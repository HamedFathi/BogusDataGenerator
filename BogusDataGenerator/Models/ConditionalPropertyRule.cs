﻿using System;
using System.Linq.Expressions;

namespace BogusDataGenerator.Models
{
    public class ConditionalPropertyRule
    {
        public string TypeName { get; set; }
        public string PropertyExpressionText { get; set; }
        public string SetterExpressionText { get; set; }

        public LambdaExpression PropertyExpression { get; set; }
        public LambdaExpression SetterExpression { get; set; }

        public Func<string, bool> Condition { get; set; }
        public string[] Locales { get; set; }

    }
}
