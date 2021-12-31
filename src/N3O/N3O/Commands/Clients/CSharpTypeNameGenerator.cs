using NJsonSchema;
using System.Collections.Generic;

namespace N3O.Commands.Clients {
    public class CSharpTypeNameGenerator : ITypeNameGenerator {
        public string Generate(JsonSchema schema, string typeNameHint, IEnumerable<string> reservedTypeNames) {
            return typeNameHint;
        }
    }
}