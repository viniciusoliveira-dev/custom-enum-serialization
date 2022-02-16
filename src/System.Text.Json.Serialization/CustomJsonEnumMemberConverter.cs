using System.Reflection;
using System.Runtime.Serialization;

namespace System.Text.Json.Serialization
{
	/// <summary>
	/// <see cref="JsonConverterFactory"/> to convert enums to and from strings, respecting <see cref="EnumMemberAttribute"/> decorations. Supports nullable enums.
	/// </summary>
	public class CustomJsonEnumMemberConverter : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert)
		{
			var canConvert = typeToConvert.IsEnum
				|| (typeToConvert.IsGenericType && TestForNullableEnum(typeToConvert).IsNullableEnum);

			return canConvert;
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			(bool isNullableEnum, Type? underlyingType) = TestForNullableEnum(typeToConvert);

			try
			{
				var converter = isNullableEnum 
					? ConverterFactory(typeof(NullableEnumMemberConverter<>), underlyingType!)
					: ConverterFactory(typeof(EnumMemberConverter<>), typeToConvert!);
				
				return converter;
			}
			catch (TargetInvocationException targetInvocationEx)
			{
				if (targetInvocationEx.InnerException != null) throw targetInvocationEx.InnerException;

				throw;
			}
		}

		private static JsonConverter ConverterFactory(Type converter, Type typeToConvert)
        {
			var jsonConverter = (JsonConverter)Activator.CreateInstance(
				converter.MakeGenericType(typeToConvert),
				BindingFlags.Instance | BindingFlags.Public,
				binder: null,
				args: Array.Empty<object>(),
				culture: null
			)!;

			return jsonConverter;
		}

		private static (bool IsNullableEnum, Type? UnderlyingType) TestForNullableEnum(Type typeToConvert)
		{
			Type? UnderlyingType = Nullable.GetUnderlyingType(typeToConvert);

			return (UnderlyingType?.IsEnum ?? false, UnderlyingType);
		}

		private class EnumMemberConverter<TEnum> : JsonConverter<TEnum>
			where TEnum : struct, Enum
		{
			private readonly CustomJsonEnumMemberConverterHelper<TEnum> _JsonStringEnumMemberConverterHelper;

			public EnumMemberConverter()
			{
				_JsonStringEnumMemberConverterHelper = new CustomJsonEnumMemberConverterHelper<TEnum>();
			}

			public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
				=> _JsonStringEnumMemberConverterHelper.Read(ref reader);

			public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
				=> _JsonStringEnumMemberConverterHelper.Write(writer, value);
		}

		private class NullableEnumMemberConverter<TEnum> : JsonConverter<TEnum?>
			where TEnum : struct, Enum
		{
			private readonly CustomJsonEnumMemberConverterHelper<TEnum> _JsonStringEnumMemberConverterHelper;

			public NullableEnumMemberConverter()
			{
				_JsonStringEnumMemberConverterHelper = new CustomJsonEnumMemberConverterHelper<TEnum>();
			}

			public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
				=> _JsonStringEnumMemberConverterHelper.Read(ref reader);

			public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
				=> _JsonStringEnumMemberConverterHelper.Write(writer, value!.Value);
		}
	}
}