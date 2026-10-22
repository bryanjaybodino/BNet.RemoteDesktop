using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace BNet.RemoteDesktop
{
    /// <summary>
    /// Tiny dependency-free JSON serializer/deserializer (replaces Newtonsoft.Json).
    /// Supports: string, bool, all numeric types, DateTime (ISO 8601), enums,
    /// Nullable&lt;T&gt;, arrays / List&lt;T&gt;, and plain classes with public get/set
    /// properties. Property names are matched case-insensitively on read.
    /// </summary>
    public static class Json
    {
        private const int MaxDepth = 64;

        // ═════════════════════════════════════════════════════════════════════
        //  Serialize
        // ═════════════════════════════════════════════════════════════════════

        public static string Serialize(object value, bool indented = false)
        {
            var sb = new StringBuilder(256);
            Write(sb, value, indented, 0);
            return sb.ToString();
        }

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propCache
            = new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static PropertyInfo[] GetProps(Type t)
        {
            return _propCache.GetOrAdd(t, type =>
            {
                var list = new List<PropertyInfo>();
                foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (p.GetIndexParameters().Length != 0) continue;
                    if (!p.CanRead || !p.CanWrite) continue;           // skips computed props
                    if (p.GetGetMethod() == null || p.GetSetMethod() == null) continue;
                    list.Add(p);
                }
                return list.ToArray();
            });
        }

        private static void Write(StringBuilder sb, object v, bool ind, int depth)
        {
            if (depth > MaxDepth) throw new InvalidOperationException("JSON too deep");

            if (v == null) { sb.Append("null"); return; }

            if (v is string s) { WriteString(sb, s); return; }
            if (v is bool b) { sb.Append(b ? "true" : "false"); return; }
            if (v is char c) { WriteString(sb, c.ToString()); return; }

            if (v is DateTime dt)
            {
                WriteString(sb, dt.ToString("o", CultureInfo.InvariantCulture));
                return;
            }
            if (v is DateTimeOffset dto)
            {
                WriteString(sb, dto.ToString("o", CultureInfo.InvariantCulture));
                return;
            }
            if (v is Guid g) { WriteString(sb, g.ToString()); return; }

            Type t = v.GetType();
            if (t.IsEnum)
            {
                sb.Append(Convert.ToInt64(v, CultureInfo.InvariantCulture)
                                 .ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (v is double d)
            {
                if (double.IsNaN(d) || double.IsInfinity(d)) sb.Append("null");
                else sb.Append(d.ToString("R", CultureInfo.InvariantCulture));
                return;
            }
            if (v is float f)
            {
                if (float.IsNaN(f) || float.IsInfinity(f)) sb.Append("null");
                else sb.Append(f.ToString("R", CultureInfo.InvariantCulture));
                return;
            }
            if (v is IFormattable num &&
                (v is int || v is long || v is short || v is byte || v is sbyte ||
                 v is uint || v is ulong || v is ushort || v is decimal))
            {
                sb.Append(num.ToString(null, CultureInfo.InvariantCulture));
                return;
            }

            if (v is IDictionary dict)
            {
                sb.Append('{');
                bool first = true;
                foreach (DictionaryEntry e in dict)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    NewLine(sb, ind, depth + 1);
                    WriteString(sb, Convert.ToString(e.Key, CultureInfo.InvariantCulture));
                    sb.Append(ind ? ": " : ":");
                    Write(sb, e.Value, ind, depth + 1);
                }
                if (!first) NewLine(sb, ind, depth);
                sb.Append('}');
                return;
            }

            if (v is IEnumerable en)
            {
                sb.Append('[');
                bool first = true;
                foreach (object item in en)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    NewLine(sb, ind, depth + 1);
                    Write(sb, item, ind, depth + 1);
                }
                if (!first) NewLine(sb, ind, depth);
                sb.Append(']');
                return;
            }

            // Plain object
            sb.Append('{');
            {
                bool first = true;
                foreach (var p in GetProps(t))
                {
                    if (!first) sb.Append(',');
                    first = false;
                    NewLine(sb, ind, depth + 1);
                    WriteString(sb, p.Name);
                    sb.Append(ind ? ": " : ":");
                    Write(sb, p.GetValue(v, null), ind, depth + 1);
                }
                if (!first) NewLine(sb, ind, depth);
            }
            sb.Append('}');
        }

        private static void NewLine(StringBuilder sb, bool ind, int depth)
        {
            if (!ind) return;
            sb.Append("\r\n");
            sb.Append(' ', depth * 2);
        }

        private static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char ch in s)
            {
                switch (ch)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (ch < 0x20)
                            sb.Append("\\u").Append(((int)ch).ToString("x4"));
                        else
                            sb.Append(ch);
                        break;
                }
            }
            sb.Append('"');
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Deserialize
        // ═════════════════════════════════════════════════════════════════════

        public static T Deserialize<T>(string json)
        {
            return (T)Deserialize(json, typeof(T));
        }

        public static object Deserialize(string json, Type type)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            var p = new Parser(json);
            object node = p.ParseDocument();
            return ConvertNode(node, type, 0);
        }

        // Parsed node types: null, string, bool, long, double,
        // List<object>, Dictionary<string, object>
        private sealed class Parser
        {
            private readonly string _s;
            private int _i;

            public Parser(string s)
            {
                _s = s;
                if (_s.Length > 0 && _s[0] == '\uFEFF') _i = 1;
            }

            public object ParseDocument()
            {
                SkipWs();
                object v = ParseValue(0);
                SkipWs();
                if (_i != _s.Length) throw Err("Unexpected trailing content");
                return v;
            }

            private Exception Err(string m) =>
                new FormatException("JSON: " + m + " at position " + _i);

            private void SkipWs()
            {
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if (c == ' ' || c == '\t' || c == '\r' || c == '\n') _i++;
                    else break;
                }
            }

            private object ParseValue(int depth)
            {
                if (depth > MaxDepth) throw Err("Nesting too deep");
                if (_i >= _s.Length) throw Err("Unexpected end");

                char c = _s[_i];
                switch (c)
                {
                    case '{': return ParseObject(depth);
                    case '[': return ParseArray(depth);
                    case '"': return ParseString();
                    case 't': Expect("true"); return true;
                    case 'f': Expect("false"); return false;
                    case 'n': Expect("null"); return null;
                    default:
                        if (c == '-' || (c >= '0' && c <= '9')) return ParseNumber();
                        throw Err("Unexpected character '" + c + "'");
                }
            }

            private void Expect(string word)
            {
                if (string.CompareOrdinal(_s, _i, word, 0, word.Length) != 0)
                    throw Err("Invalid literal");
                _i += word.Length;
            }

            private object ParseObject(int depth)
            {
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _i++; // {
                SkipWs();
                if (_i < _s.Length && _s[_i] == '}') { _i++; return dict; }

                while (true)
                {
                    SkipWs();
                    if (_i >= _s.Length || _s[_i] != '"') throw Err("Expected property name");
                    string key = ParseString();
                    SkipWs();
                    if (_i >= _s.Length || _s[_i] != ':') throw Err("Expected ':'");
                    _i++;
                    SkipWs();
                    dict[key] = ParseValue(depth + 1);
                    SkipWs();
                    if (_i >= _s.Length) throw Err("Unexpected end");
                    if (_s[_i] == ',') { _i++; continue; }
                    if (_s[_i] == '}') { _i++; return dict; }
                    throw Err("Expected ',' or '}'");
                }
            }

            private object ParseArray(int depth)
            {
                var list = new List<object>();
                _i++; // [
                SkipWs();
                if (_i < _s.Length && _s[_i] == ']') { _i++; return list; }

                while (true)
                {
                    SkipWs();
                    list.Add(ParseValue(depth + 1));
                    SkipWs();
                    if (_i >= _s.Length) throw Err("Unexpected end");
                    if (_s[_i] == ',') { _i++; continue; }
                    if (_s[_i] == ']') { _i++; return list; }
                    throw Err("Expected ',' or ']'");
                }
            }

            private string ParseString()
            {
                _i++; // opening quote
                var sb = new StringBuilder();
                while (true)
                {
                    if (_i >= _s.Length) throw Err("Unterminated string");
                    char c = _s[_i++];
                    if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }

                    if (_i >= _s.Length) throw Err("Bad escape");
                    char e = _s[_i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 > _s.Length) throw Err("Bad \\u escape");
                            int code;
                            if (!int.TryParse(_s.Substring(_i, 4), NumberStyles.AllowHexSpecifier,
                                              CultureInfo.InvariantCulture, out code))
                                throw Err("Bad \\u escape");
                            sb.Append((char)code);
                            _i += 4;
                            break;
                        default: throw Err("Bad escape");
                    }
                }
            }

            private object ParseNumber()
            {
                int start = _i;
                bool isFloat = false;
                if (_s[_i] == '-') _i++;
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if (c >= '0' && c <= '9') _i++;
                    else if (c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-')
                    { isFloat = true; _i++; }
                    else break;
                }
                string tok = _s.Substring(start, _i - start);
                if (!isFloat)
                {
                    long l;
                    if (long.TryParse(tok, NumberStyles.AllowLeadingSign,
                                      CultureInfo.InvariantCulture, out l))
                        return l;
                }
                double d;
                if (double.TryParse(tok, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                    return d;
                throw Err("Bad number");
            }
        }

        // ── Node → target type ────────────────────────────────────────────────

        private static object ConvertNode(object node, Type t, int depth)
        {
            if (depth > MaxDepth) throw new FormatException("JSON too deep");

            Type under = Nullable.GetUnderlyingType(t);
            if (node == null)
                return (t.IsValueType && under == null) ? Activator.CreateInstance(t) : null;
            if (under != null) t = under;

            if (t == typeof(object)) return node;

            if (t == typeof(string))
                return node is string ? node : Convert.ToString(node, CultureInfo.InvariantCulture);

            if (t == typeof(bool))
            {
                if (node is bool) return node;
                if (node is string sb) return bool.Parse(sb);
                return Convert.ToInt64(node, CultureInfo.InvariantCulture) != 0;
            }

            if (t == typeof(DateTime))
            {
                if (node is string ds)
                    return DateTime.Parse(ds, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                throw new FormatException("JSON: expected date string");
            }
            if (t == typeof(DateTimeOffset))
            {
                if (node is string ds2)
                    return DateTimeOffset.Parse(ds2, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                throw new FormatException("JSON: expected date string");
            }
            if (t == typeof(Guid))
                return Guid.Parse(Convert.ToString(node, CultureInfo.InvariantCulture));

            if (t.IsEnum)
            {
                if (node is string es) return Enum.Parse(t, es, true);
                return Enum.ToObject(t, Convert.ToInt64(node, CultureInfo.InvariantCulture));
            }

            if (t.IsPrimitive || t == typeof(decimal))
            {
                if (node is string ns)
                    return Convert.ChangeType(ns, t, CultureInfo.InvariantCulture);
                return Convert.ChangeType(node, t, CultureInfo.InvariantCulture);
            }

            // Arrays / lists
            if (t.IsArray)
            {
                var src = node as List<object>;
                if (src == null) throw new FormatException("JSON: expected array");
                Type et = t.GetElementType();
                Array arr = Array.CreateInstance(et, src.Count);
                for (int i = 0; i < src.Count; i++)
                    arr.SetValue(ConvertNode(src[i], et, depth + 1), i);
                return arr;
            }

            if (t.IsGenericType)
            {
                Type gd = t.GetGenericTypeDefinition();
                if (gd == typeof(List<>) || gd == typeof(IList<>) ||
                    gd == typeof(IEnumerable<>) || gd == typeof(ICollection<>) ||
                    gd == typeof(IReadOnlyList<>))
                {
                    var src = node as List<object>;
                    if (src == null) throw new FormatException("JSON: expected array");
                    Type et = t.GetGenericArguments()[0];
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(et));
                    foreach (object item in src)
                        list.Add(ConvertNode(item, et, depth + 1));
                    return list;
                }
                if (gd == typeof(Dictionary<,>) || gd == typeof(IDictionary<,>))
                {
                    var src = node as Dictionary<string, object>;
                    if (src == null) throw new FormatException("JSON: expected object");
                    Type[] ga = t.GetGenericArguments();
                    if (ga[0] != typeof(string)) throw new NotSupportedException("Only string keys");
                    var dict = (IDictionary)Activator.CreateInstance(
                        typeof(Dictionary<,>).MakeGenericType(ga));
                    foreach (var kv in src)
                        dict[kv.Key] = ConvertNode(kv.Value, ga[1], depth + 1);
                    return dict;
                }
            }

            // Plain class
            var obj = node as Dictionary<string, object>;
            if (obj == null) throw new FormatException("JSON: expected object for " + t.Name);

            object result = Activator.CreateInstance(t);
            foreach (var p in GetProps(t))
            {
                object raw;
                if (obj.TryGetValue(p.Name, out raw))
                    p.SetValue(result, ConvertNode(raw, p.PropertyType, depth + 1), null);
            }
            return result;
        }
    }
}