using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System;

namespace Fiver.Api.HATEOAS.Lib
{
    public class ContentTypeOf : Attribute, IActionConstraint
    {
        private readonly string expectedContentType;

        public ContentTypeOf(string expectedContentType)
        {
            this.expectedContentType = expectedContentType;
        }

        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            var request = context.RouteContext.HttpContext.Request;

            if (!request.Headers.ContainsKey("Content-Type"))
                return false;
            
            return string.Equals(request.Headers["Content-Type"], 
                                    expectedContentType, 
                                    StringComparison.OrdinalIgnoreCase);
        }
    }
}
