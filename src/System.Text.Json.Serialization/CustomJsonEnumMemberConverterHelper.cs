using System.Net;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Text.Json.Serialization
{
    internal class CustomJsonEnumMemberConverterHelper<TEnum> where TEnum : struct, Enum
    {
        private const BindingFlags _enumBindings = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
        private readonly Dictionary<TEnum, EnumInfo<TEnum>> _currentEnumValues;

        public CustomJsonEnumMemberConverterHelper()
        {
            Type enumType = typeof(TEnum);

            string[] enumNames = enumType.GetEnumNames();
            Array builtInValues = enumType.GetEnumValues();

            _currentEnumValues = new Dictionary<TEnum, EnumInfo<TEnum>>(enumNames.Length);

            MapEnumValues(enumType, enumNames, builtInValues);
        }

        private void MapEnumValues(Type enumType, string[] enumNames, Array builtInValues)
        {
            for (int i = 0; i < enumNames.Length; i++)
            {
                Enum? CurrentEnumValue = (Enum?)builtInValues.GetValue(i);
                if (CurrentEnumValue is null) continue;

                int enumNumber = GetEnumValue(CurrentEnumValue);

                FieldInfo field = enumType.GetField(enumNames[i], _enumBindings)!;

                string enumMemberText = field.GetCustomAttribute<EnumMemberAttribute>(true)?.Value!;

                if (CurrentEnumValue is not TEnum enumValue) throw new NotSupportedException();

                _currentEnumValues[enumValue] = new EnumInfo<TEnum>(enumMemberText, enumValue, enumNumber);
            }
        }

        private static int GetEnumValue(object value)
        {
            var enumValue = (int)value;

            return enumValue;
        }

        internal TEnum Read(ref Utf8JsonReader reader)
        {
            JsonTokenType tokenType = reader.TokenType;

            string enumMemberTextOrEnumNumber = tokenType == JsonTokenType.String
                ? reader.GetString()!
                : reader.GetInt32()!.ToString();

            EnumInfo<TEnum>? enumInfo = _currentEnumValues.Values.FirstOrDefault(x => x.EnumMemberText == enumMemberTextOrEnumNumber);
            if (enumInfo is not null)
            {
                return enumInfo.EnumValue;
            }

            else if (Enum.TryParse(enumMemberTextOrEnumNumber, out TEnum result))
            {
                return result;
            }

            throw new NotSupportedException($"Value {enumMemberTextOrEnumNumber} not supported.");
        }

        internal void Write(Utf8JsonWriter writer, TEnum value)
        {
            if (_currentEnumValues.TryGetValue(value, out EnumInfo<TEnum>? enumInfo))
            {
                if (value is HttpStatusCode || enumInfo.EnumMemberText is null)
                {
                    writer.WriteNumberValue(enumInfo.EnumNumber);
                }
                else
                {
                    writer.WriteStringValue(enumInfo.EnumMemberText);
                }

                return;
            }

            throw new NotSupportedException($"Value {value} not supported.");
        }
    }
}