﻿//---------------------------------------------------------------------
// <copyright file="PropertyAndValueJsonWriterIntegrationTests.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Json;
using Xunit;

namespace Microsoft.OData.Tests.IntegrationTests.Writer.Json
{
    public class PropertyAndValueJsonWriterIntegrationTests
    {
        [Fact]
        public void WritingPayloadInt64SingleDoubleDecimalWithoutSuffix()
        {
            EdmModel model = new EdmModel();
            EdmEntityType entityType = new EdmEntityType("NS", "MyTestEntity");
            EdmStructuralProperty key = entityType.AddStructuralProperty("LongId", EdmPrimitiveTypeKind.Int64, false);
            entityType.AddKeys(key);
            entityType.AddStructuralProperty("FloatId", EdmPrimitiveTypeKind.Single, false);
            entityType.AddStructuralProperty("DoubleId", EdmPrimitiveTypeKind.Double, false);
            entityType.AddStructuralProperty("DecimalId", EdmPrimitiveTypeKind.Decimal, false);
            entityType.AddStructuralProperty("BoolValue1", EdmPrimitiveTypeKind.Boolean, false);
            entityType.AddStructuralProperty("BoolValue2", EdmPrimitiveTypeKind.Boolean, false);

            EdmComplexType complexType = new EdmComplexType("NS", "MyTestComplexType");
            complexType.AddStructuralProperty("CLongId", EdmPrimitiveTypeKind.Int64, false);
            complexType.AddStructuralProperty("CFloatId", EdmPrimitiveTypeKind.Single, false);
            complexType.AddStructuralProperty("CDoubleId", EdmPrimitiveTypeKind.Double, false);
            complexType.AddStructuralProperty("CDecimalId", EdmPrimitiveTypeKind.Decimal, false);
            model.AddElement(complexType);
            EdmComplexTypeReference complexTypeRef = new EdmComplexTypeReference(complexType, true);
            entityType.AddStructuralProperty("ComplexProperty", complexTypeRef);

            entityType.AddStructuralProperty("LongNumbers", new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetInt64(false))));
            entityType.AddStructuralProperty("FloatNumbers", new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetSingle(false))));
            entityType.AddStructuralProperty("DoubleNumbers", new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetDouble(false))));
            entityType.AddStructuralProperty("DecimalNumbers", new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetDecimal(false))));
            model.AddElement(entityType);

            EdmEntityContainer container = new EdmEntityContainer("EntityNs", "MyContainer_sub");
            EdmEntitySet entitySet = new EdmEntitySet(container, "MyTestEntitySet", entityType);
            model.AddElement(container);

            ODataResource entry = new ODataResource()
            {
                Id = new Uri("http://mytest"),
                TypeName = "NS.MyTestEntity",
                Properties = new[]
                {
                    new ODataProperty {Name = "LongId", Value = 12L},
                    new ODataProperty {Name = "FloatId", Value = 34.98f},
                    new ODataProperty {Name = "DoubleId", Value = 56.010},
                    new ODataProperty {Name = "DecimalId", Value = 78.62m},
                    new ODataProperty {Name = "BoolValue1", Value = true},
                    new ODataProperty {Name = "BoolValue2", Value = false},
                    new ODataProperty {Name = "LongNumbers", Value = new ODataCollectionValue {Items = new object[] {0L, long.MinValue, long.MaxValue}, TypeName = "Collection(Int64)" }},
                    new ODataProperty {Name = "FloatNumbers", Value = new ODataCollectionValue {Items = new object[] {1F, float.MinValue, float.MaxValue, float.PositiveInfinity, float.NegativeInfinity, float.NaN}, TypeName = "Collection(Single)" }},
                    new ODataProperty {Name = "DoubleNumbers", Value = new ODataCollectionValue {Items = new object[] {-1D, double.MinValue, double.MaxValue, double.PositiveInfinity, double.NegativeInfinity, double.NaN}, TypeName = "Collection(Double)" }},
                    new ODataProperty {Name = "DecimalNumbers", Value = new ODataCollectionValue {Items = new object[] {0M, decimal.MinValue, decimal.MaxValue}, TypeName = "Collection(Decimal)" }},
                },
            };

            var complexResourceInfo = new ODataNestedResourceInfo
            {
                Name = "ComplexProperty",
                IsCollection = false
            };

            var complexResource = new ODataResource
            {
                TypeName = "NS.MyTestComplexType",
                Properties = new[]
                {
                    new ODataProperty { Name = "CLongId", Value = 1L},
                    new ODataProperty { Name = "CFloatId", Value = -1.0F},
                    new ODataProperty { Name = "CDoubleId", Value = 1.0D},
                    new ODataProperty { Name = "CDecimalId", Value = 1.0M},
                }
            };

            string outputPayload = this.WriterEntry(
                TestUtils.WrapReferencedModelsToMainModel("EntityNs", "MyContainer", model),
                entry, entitySet, entityType, false,
                (writer) =>
                {
                    writer.WriteStart(entry);
                    writer.WriteStart(complexResourceInfo);
                    writer.WriteStart(complexResource);
                    writer.WriteEnd();
                    writer.WriteEnd();
                    writer.WriteEnd();
                });

            string expectedPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#MyTestEntitySet/$entity\"," +
                "\"@odata.id\":\"http://mytest/\"," +
                "\"LongId\":12," +
                "\"FloatId\":34.98," +
                "\"DoubleId\":56.01," +
                "\"DecimalId\":78.62," +
                "\"BoolValue1\":true," +
                "\"BoolValue2\":false," +
                "\"LongNumbers\":[" +
                "0," +
                "-9223372036854775808," +
                "9223372036854775807" +
                "]," +
                "\"FloatNumbers\":[" +
                "1," +
                "-3.4028235E+38," +
                "3.4028235E+38," +
                "\"INF\"," +
                "\"-INF\"," +
                "\"NaN\"" +
                "]," +
                "\"DoubleNumbers\":[" +
                "-1.0," +
                "-1.7976931348623157E+308," +
                "1.7976931348623157E+308," +
                "\"INF\"," +
                "\"-INF\"," +
                "\"NaN\"" +
                "]," +
                "\"DecimalNumbers\":[" +
                "0," +
                "-79228162514264337593543950335," +
                "79228162514264337593543950335" +
                "]," +
                "\"ComplexProperty\":{" +
                "\"CLongId\":1," +
                "\"CFloatId\":-1," +
                "\"CDoubleId\":1.0," +
                "\"CDecimalId\":1.0}" +
                "}";

            Assert.Equal(expectedPayload, outputPayload);
        }

        [Fact]
        public void WriteTypeDefinitionPayloadShouldWork()
        {
            EdmModel model = new EdmModel();

            EdmEntityType entityType = new EdmEntityType("NS", "Person");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            EdmTypeDefinition weightType = new EdmTypeDefinition("NS", "Weight", EdmPrimitiveTypeKind.Double);
            EdmTypeDefinitionReference weightTypeRef = new EdmTypeDefinitionReference(weightType, false);
            entityType.AddStructuralProperty("Weight", weightTypeRef);

            EdmComplexType complexType = new EdmComplexType("NS", "Address");
            EdmComplexTypeReference complexTypeRef = new EdmComplexTypeReference(complexType, true);

            EdmTypeDefinition addressType = new EdmTypeDefinition("NS", "Address", EdmPrimitiveTypeKind.String);
            EdmTypeDefinitionReference addressTypeRef = new EdmTypeDefinitionReference(addressType, false);
            complexType.AddStructuralProperty("CountryRegion", addressTypeRef);

            entityType.AddStructuralProperty("Address", complexTypeRef);

            model.AddElement(weightType);
            model.AddElement(addressType);
            model.AddElement(complexType);
            model.AddElement(entityType);

            EdmEntityContainer container = new EdmEntityContainer("EntityNs", "MyContainer");
            EdmEntitySet entitySet = container.AddEntitySet("People", entityType);
            model.AddElement(container);

            ODataResource entry = new ODataResource()
            {
                TypeName = "NS.Person",
                Properties = new[]
                {
                    new ODataProperty { Name = "Id", Value = 1 },
                    new ODataProperty { Name = "Weight", Value = 60.5 }
                }
            };

            ODataNestedResourceInfo address = new ODataNestedResourceInfo()
            {
                Name = "Address",
                IsCollection = false
            };

            ODataResource addressResource = new ODataResource
            {
                Properties = new[]
                {
                    new ODataProperty { Name = "CountryRegion", Value = "China" }
                }
            };

            string outputPayload = this.WriterEntry(model, entry, entitySet, entityType, false, (writer)
                =>
                {
                    writer.WriteStart(entry);
                    writer.WriteStart(address);
                    writer.WriteStart(addressResource);
                    writer.WriteEnd();
                    writer.WriteEnd();
                    writer.WriteEnd();
                });

            const string expectedMinimalPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Weight\":60.5," +
                "\"Address\":{\"CountryRegion\":\"China\"}" +
                "}";

            Assert.Equal(expectedMinimalPayload, outputPayload);

            outputPayload = this.WriterEntry(model, entry, entitySet, entityType, true, (writer)
                =>
                {
                    writer.WriteStart(entry);
                    writer.WriteStart(address);
                    writer.WriteStart(addressResource);
                    writer.WriteEnd();
                    writer.WriteEnd();
                    writer.WriteEnd();
                });

            const string expectedFullPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"@odata.type\":\"#NS.Person\"," +
                "\"@odata.id\":\"People(1)\"," +
                "\"@odata.editLink\":\"People(1)\"," +
                "\"Id\":1," +
                "\"Weight\":60.5," +
                "\"Address\":{\"CountryRegion\":\"China\"}" +
                "}";

            Assert.Equal(expectedFullPayload, outputPayload);
        }

        [Fact]
        public void WriteTypeDefinitionAsKeyPayloadShouldWork()
        {
            EdmModel model = new EdmModel();

            EdmTypeDefinition uint32Type = new EdmTypeDefinition("NS", "UInt32", EdmPrimitiveTypeKind.String);
            EdmTypeDefinitionReference uint32Reference = new EdmTypeDefinitionReference(uint32Type, false);
            model.AddElement(uint32Type);

            EdmEntityType entityType = new EdmEntityType("NS", "Person");
            entityType.AddKeys(entityType.AddStructuralProperty("Id1", uint32Reference), entityType.AddStructuralProperty("Id2", EdmPrimitiveTypeKind.Int32));
            model.SetPrimitiveValueConverter(uint32Reference, new MyUInt32Converter());

            EdmTypeDefinition weightType = new EdmTypeDefinition("NS", "Weight", EdmPrimitiveTypeKind.Double);
            EdmTypeDefinitionReference weightTypeRef = new EdmTypeDefinitionReference(weightType, false);
            entityType.AddStructuralProperty("Weight", weightTypeRef);

            EdmComplexType complexType = new EdmComplexType("NS", "Address");
            EdmComplexTypeReference complexTypeRef = new EdmComplexTypeReference(complexType, true);

            EdmTypeDefinition addressType = new EdmTypeDefinition("NS", "Address", EdmPrimitiveTypeKind.String);
            EdmTypeDefinitionReference addressTypeRef = new EdmTypeDefinitionReference(addressType, false);
            complexType.AddStructuralProperty("CountryRegion", addressTypeRef);

            entityType.AddStructuralProperty("Address", complexTypeRef);

            model.AddElement(weightType);
            model.AddElement(addressType);
            model.AddElement(complexType);
            model.AddElement(entityType);

            EdmEntityContainer container = new EdmEntityContainer("EntityNs", "MyContainer");
            EdmEntitySet entitySet = container.AddEntitySet("People", entityType);
            model.AddElement(container);

            ODataResource entry = new ODataResource()
            {
                TypeName = "NS.Person",
                Properties = new[]
                {
                    new ODataProperty { Name = "Id1", Value = (UInt32)1 },
                    new ODataProperty { Name = "Id2", Value = 2 },
                    new ODataProperty { Name = "Weight", Value = 60.5 },
                }
            };

            ODataNestedResourceInfo address = new ODataNestedResourceInfo()
            {
                Name = "Address",
                IsCollection = false
            };

            ODataResource addressResource = new ODataResource
            {
                Properties = new[]
                {
                    new ODataProperty { Name = "CountryRegion", Value = "China" }
                }
            };

            string outputPayload = this.WriterEntry(model, entry, entitySet, entityType, false, (writer)
                =>
            {
                writer.WriteStart(entry);
                writer.WriteStart(address);
                writer.WriteStart(addressResource);
                writer.WriteEnd();
                writer.WriteEnd();
                writer.WriteEnd();
            });

            const string expectedMinimalPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"Id1\":\"1\"," +
                "\"Id2\":2," +
                "\"Weight\":60.5," +
                "\"Address\":{\"CountryRegion\":\"China\"}" +
                "}";

            Assert.Equal(expectedMinimalPayload, outputPayload);

            outputPayload = this.WriterEntry(model, entry, entitySet, entityType, true, (writer)
                =>
                {
                    writer.WriteStart(entry);
                    writer.WriteStart(address);
                    writer.WriteStart(addressResource);
                    writer.WriteEnd();
                    writer.WriteEnd();
                    writer.WriteEnd();
                });

            const string expectedFullPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"@odata.type\":\"#NS.Person\"," +
                "\"@odata.id\":\"People(Id1='1',Id2=2)\"," +
                "\"@odata.editLink\":\"People(Id1='1',Id2=2)\"," +
                "\"Id1\":\"1\"," +
                "\"Id2\":2," +
                "\"Weight\":60.5," +
                "\"Address\":{\"CountryRegion\":\"China\"}" +
                "}";

            Assert.Equal(expectedFullPayload, outputPayload);
        }

        [Fact]
        public void WriteTypeDefinitionPayloadWithIncompatibleTypeShouldFail()
        {
            EdmModel model = new EdmModel();

            EdmEntityType entityType = new EdmEntityType("NS", "Person");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            EdmTypeDefinition weightType = new EdmTypeDefinition("NS", "Weight", EdmPrimitiveTypeKind.Double);
            EdmTypeDefinitionReference weightTypeRef = new EdmTypeDefinitionReference(weightType, false);
            entityType.AddStructuralProperty("Weight", weightTypeRef);

            model.AddElement(weightType);
            model.AddElement(entityType);

            EdmEntityContainer container = new EdmEntityContainer("EntityNs", "MyContainer");
            EdmEntitySet entitySet = container.AddEntitySet("People", entityType);
            model.AddElement(container);

            ODataResource entry = new ODataResource()
            {
                TypeName = "NS.Person",
                Properties = new[]
                {
                    new ODataProperty { Name = "Id", Value = 1 },
                    new ODataProperty { Name = "Weight", Value = "abc" },
                }
            };

            Action write = () => this.WriterEntry(model, entry, entitySet, entityType);
            write.Throws<ODataException>(Error.Format(SRResources.ValidationUtils_IncompatiblePrimitiveItemType, "Edm.String", "True", "NS.Weight", "False"));
        }

        [Fact]
        public void WriteUIntPayloadShouldWork()
        {
            EdmModel model = new EdmModel();

            EdmEntityType entityType = new EdmEntityType("NS", "Person");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            entityType.AddStructuralProperty("Guid", model.GetUInt64("NS", false));

            model.AddElement(entityType);

            EdmEntityContainer container = new EdmEntityContainer("Ns", "MyContainer");
            EdmEntitySet entitySet = container.AddEntitySet("People", entityType);
            model.AddElement(container);

            ODataResource entry = new ODataResource()
            {
                TypeName = "NS.Person",
                Properties = new[]
                {
                    new ODataProperty { Name = "Id", Value = 1 },
                    new ODataProperty { Name = "Guid", Value = UInt64.MaxValue }
                }
            };

            string outputPayload = this.WriterEntry(model, entry, entitySet, entityType);

            const string expectedMinimalPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Guid\":18446744073709551615" +
                "}";

            Assert.Equal(expectedMinimalPayload, outputPayload);

            outputPayload = this.WriterEntry(model, entry, entitySet, entityType, true);

            const string expectedFullPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"@odata.type\":\"#NS.Person\"," +
                "\"@odata.id\":\"People(1)\"," +
                "\"@odata.editLink\":\"People(1)\"," +
                "\"Id\":1," +
                "\"Guid\":18446744073709551615" +
                "}";

            Assert.Equal(expectedFullPayload, outputPayload);
        }

        [Fact]
        public void WriteEntryWithStringEscapeOptionShouldWork()
        {
            EdmModel model = new EdmModel();

            EdmEntityType entityType = new EdmEntityType("NS", "Person");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("Amount", EdmPrimitiveTypeKind.Double);
            model.AddElement(entityType);

            EdmEntityContainer container = new EdmEntityContainer("Ns", "MyContainer");
            EdmEntitySet entitySet = container.AddEntitySet("People", entityType);
            model.AddElement(container);

            ODataResource entry = new ODataResource()
            {
                TypeName = "NS.Person",
                Properties = new[]
                {
                    new ODataProperty { Name = "Id", Value = 1 },
                    new ODataProperty { Name = "Name", Value = "и\nя" },
                    new ODataProperty { Name = "Amount", Value = 300.0 }
                }
            };

            // 1. without specifying JavaScriptEncoder
            string outputPayload = this.WriterEntry(model, entry, entitySet, entityType, false, null, jsonWriterFactory: null);

            const string expectedEscapedOnlyControlPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Name\":\"и\\nя\"," +
                "\"Amount\":300.0" +
                "}";
            Assert.Equal(expectedEscapedOnlyControlPayload, outputPayload);


            // 2. With JavaScriptEncoder.Default (escapes non-ascii character)
            const string expectedEscapedNonAsciiPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Name\":\"\\u0438\\n\\u044F\"," +
                "\"Amount\":300.0" +
                "}";
            outputPayload = this.WriterEntry(model, entry, entitySet, entityType, false, null, new ODataUtf8JsonWriterFactory(JavaScriptEncoder.Default));

            Assert.Equal(expectedEscapedNonAsciiPayload, outputPayload);

            // 3. With JavaScriptEncoder.UnsafeRelaxedJsonEscaping, escapes control control characters
            outputPayload = this.WriterEntry(model, entry, entitySet, entityType,
                false, null, new ODataUtf8JsonWriterFactory(JavaScriptEncoder.UnsafeRelaxedJsonEscaping));
            Assert.Equal(expectedEscapedOnlyControlPayload, outputPayload);
        }

        [Fact]
        public void WriteEntryWithStringEscapeOptionShouldWorkUsingODataJsonWriterFactory()
        {
            EdmModel model = new EdmModel();

            EdmEntityType entityType = new EdmEntityType("NS", "Person");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("Amount", EdmPrimitiveTypeKind.Double);
            model.AddElement(entityType);

            EdmEntityContainer container = new EdmEntityContainer("Ns", "MyContainer");
            EdmEntitySet entitySet = container.AddEntitySet("People", entityType);
            model.AddElement(container);

            ODataResource entry = new ODataResource()
            {
                TypeName = "NS.Person",
                Properties = new[]
                {
                    new ODataProperty { Name = "Id", Value = 1 },
                    new ODataProperty { Name = "Name", Value = "и\nя" },
                    new ODataProperty { Name = "Amount", Value = 300.0 }
                }
            };

            // 1. without string escape option
            string outputPayload = this.WriterEntry(model, entry, entitySet, entityType, false, null, new ODataJsonWriterFactory());

            const string expectedEscapedNonAsciiPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Name\":\"\\u0438\\n\\u044f\"," +
                "\"Amount\":300.0" +
                "}";
            Assert.Equal(expectedEscapedNonAsciiPayload, outputPayload);

            // 2. With EscapeNonAscii escape option
            outputPayload = this.WriterEntry(model, entry, entitySet, entityType, false, null, new ODataJsonWriterFactory(ODataStringEscapeOption.EscapeNonAscii));

            Assert.Equal(expectedEscapedNonAsciiPayload, outputPayload);

            // 3. With EscapeOnlyControls escape option
            outputPayload = this.WriterEntry(model, entry, entitySet, entityType,
                false, null, new ODataJsonWriterFactory(ODataStringEscapeOption.EscapeOnlyControls));

            const string expectedEscapedOnlyControlPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Name\":\"и\\nя\"," +
                 "\"Amount\":300.0" +
                "}";
            Assert.Equal(expectedEscapedOnlyControlPayload, outputPayload);
        }

        [Fact]
        public void WriteEntryWithEscapeOnlyControlsOptionShouldWork()
        {
            EdmModel model = new EdmModel();

            EdmEntityType entityType = new EdmEntityType("NS", "Person");
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            entityType.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(entityType);

            EdmEntityContainer container = new EdmEntityContainer("Ns", "MyContainer");
            EdmEntitySet entitySet = container.AddEntitySet("People", entityType);
            model.AddElement(container);

            ODataResource entry = new ODataResource()
            {
                TypeName = "NS.Person",
                Properties = new[]
                {
                    new ODataProperty { Name = "Id", Value = 1 },
                    new ODataProperty { Name = "Name", Value = "и\nя" }
                }
            };

            string outputPayload = this.WriterEntry(model, entry, entitySet, entityType,
                false, null, new ODataUtf8JsonWriterFactory(JavaScriptEncoder.UnsafeRelaxedJsonEscaping));

            const string expectedMinimalPayload =
                "{" +
                "\"@odata.context\":\"http://www.example.com/$metadata#People/$entity\"," +
                "\"Id\":1," +
                "\"Name\":\"и\\nя\"" +
                "}";
            Assert.Equal(expectedMinimalPayload, outputPayload);
        }

        [Fact]
        public void WriteDynamicPropertyOfUIntIsNotSupported()
        {
            EdmModel model = new EdmModel();

            EdmEntityType entityType = new EdmEntityType("NS", "Person", null, false, true);
            entityType.AddKeys(entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            model.AddElement(entityType);

            EdmEntityContainer container = new EdmEntityContainer("Ns", "MyContainer");
            EdmEntitySet entitySet = container.AddEntitySet("People", entityType);
            model.AddElement(container);

            ODataResource entry = new ODataResource()
            {
                TypeName = "NS.Person",
                Properties = new[]
                {
                    new ODataProperty { Name = "Id", Value = 1 },
                    new ODataProperty { Name = "Guid", Value = UInt64.MaxValue }
                }
            };

            Action write = () => this.WriterEntry(model, entry, entitySet, entityType);
            write.Throws<ODataException>("The value of type 'System.UInt64' is not supported and cannot be converted to a JSON representation.");
        }

        private string WriterEntry(IEdmModel userModel, ODataResource entry, EdmEntitySet entitySet, IEdmEntityType entityType,
            bool fullMetadata = false, Action<ODataWriter> writeAction = null, IJsonWriterFactory jsonWriterFactory = null)
        {
            var message = new InMemoryMessage() { Stream = new MemoryStream() };
            if (jsonWriterFactory != null)
            {
                IServiceCollection services = new ServiceCollection().AddDefaultODataServices();
                services.AddSingleton<IJsonWriterFactory>(sp => jsonWriterFactory);
                message.ServiceProvider = services.BuildServiceProvider();
            }

            var writerSettings = new ODataMessageWriterSettings { EnableMessageStreamDisposal = false };

            writerSettings.SetContentType(ODataFormat.Json);
            writerSettings.SetServiceDocumentUri(new Uri("http://www.example.com"));
            writerSettings.SetContentType(fullMetadata ?
                "application/json;odata.metadata=full;odata.streaming=false" :
                "application/json;odata.metadata=minimal;odata.streaming=false", "utf-8");

            using (var msgReader = new ODataMessageWriter((IODataResponseMessage)message, writerSettings, userModel))
            {
                var writer = msgReader.CreateODataResourceWriter(entitySet, entityType);
                if (writeAction != null)
                {
                    writeAction(writer);
                }
                else
                {
                    writer.WriteStart(entry);
                    writer.WriteEnd();
                }
            }

            message.Stream.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(message.Stream))
            {
                return reader.ReadToEnd();
            }
        }

        private class MyUInt32Converter : IPrimitiveValueConverter
        {
            public object ConvertToUnderlyingType(object value)
            {
                return Convert.ToString(value);
            }

            public object ConvertFromUnderlyingType(object value)
            {
                return Convert.ToUInt32(value);
            }
        }
    }
}
