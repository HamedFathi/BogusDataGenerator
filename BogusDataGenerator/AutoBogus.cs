using Bogus;
using BogusDataGenerator.Extensions;
using BogusDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BogusDataGenerator
{
    public static class AutoBogus
    {
        private readonly static Faker faker = new Faker();
        private static object GetResult(this LambdaExpression lambdaExpression)
        {

            var types = lambdaExpression.Parameters.Select(x => x.Type).ToArray();
            if (types.Length == 1)
            {
                return lambdaExpression.Compile().DynamicInvoke(faker);
            }
            else
            {
                return lambdaExpression.Compile().DynamicInvoke(faker, Activator.CreateInstance(types[1]));
            }
        }


        public static List<T> Generate<T>(int count = 1, params BogusData[] bogusData)
        {           
            var type = typeof(T);
            var innerTypes = type.GetInnerTypes();

            foreach (var item in innerTypes)
            {
                
            }


            return null;
        }
    }
}
