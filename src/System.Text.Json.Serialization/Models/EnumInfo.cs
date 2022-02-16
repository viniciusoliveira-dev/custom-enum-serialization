namespace System.Text.Json.Serialization
{
    internal class EnumInfo<TEnum> where TEnum : struct, Enum
    {
        public string EnumMemberText;
        public TEnum EnumValue;
        public int EnumNumber;

        public EnumInfo(string enumMemberText, TEnum enumValue, int enumNumber)
        {
            EnumMemberText = enumMemberText;
            EnumValue = enumValue;
            EnumNumber = enumNumber;
        }
    }
}
