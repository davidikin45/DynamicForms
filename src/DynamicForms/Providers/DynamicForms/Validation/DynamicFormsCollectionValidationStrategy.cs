using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicForms.Providers.DynamicForms.Validation
{
    public class DynamicFormsCollectionValidationStrategy : IValidationStrategy
    {
        private static readonly MethodInfo _getEnumerator = typeof(DynamicFormsCollectionValidationStrategy)
            .GetMethod(nameof(GetEnumerator), BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Gets an instance of <see cref="DefaultCollectionValidationStrategy"/>.
        /// </summary>
        public static readonly DynamicFormsCollectionValidationStrategy Instance = new DynamicFormsCollectionValidationStrategy();
        private readonly ConcurrentDictionary<Type, Func<object, IEnumerator>> _genericGetEnumeratorCache = new ConcurrentDictionary<Type, Func<object, IEnumerator>>();

        private DynamicFormsCollectionValidationStrategy()
        {
        }

        /// <inheritdoc />
        public IEnumerator<ValidationEntry> GetChildren(
            ModelMetadata metadata,
            string key,
            object model)
        {
            var enumerator = GetEnumeratorForElementType(metadata, model);

           if(metadata.ElementType.GetInterfaces().Contains(typeof(ICustomTypeDescriptor)))
            {
                var metadataList = new List<ModelMetadata>();

                while(enumerator.MoveNext())
                {
                    metadataList.Add(metadata.ElementMetadata);
                }
                enumerator.Reset();
                return new Enumerator(null, metadataList, key, enumerator);
            }
           else
            {
                return new Enumerator(metadata.ElementMetadata, null, key, enumerator);
            }
        }

        public IEnumerator GetEnumeratorForElementType(ModelMetadata metadata, object model)
        {
            Func<object, IEnumerator> getEnumerator = _genericGetEnumeratorCache.GetOrAdd(
                    key: metadata.ElementType,
                    valueFactory: (type) => {
                        var getEnumeratorMethod = _getEnumerator.MakeGenericMethod(type);
                        var parameter = Expression.Parameter(typeof(object), "model");
                        var expression =
                            Expression.Lambda<Func<object, IEnumerator>>(
                                Expression.Call(null, getEnumeratorMethod, parameter),
                                parameter);
                        return expression.Compile();
                    });

            return getEnumerator(model);
        }

        // Called via reflection.
        private static IEnumerator GetEnumerator<T>(object model)
        {
            return (model as IEnumerable<T>)?.GetEnumerator() ?? ((IEnumerable)model).GetEnumerator();
        }

        private class Enumerator : IEnumerator<ValidationEntry>
        {
            private readonly string _key;
            private readonly ModelMetadata _metadata;
            private readonly List<ModelMetadata> _metadataList;
            private readonly IEnumerator _enumerator;

            private ValidationEntry _entry;
            private int _index;

            public Enumerator(
                ModelMetadata metadata,
                List<ModelMetadata> metadataList,
                string key,
                IEnumerator enumerator)
            {
                _metadata = metadata;
                _metadataList = metadataList;
                _key = key;
                _enumerator = enumerator;

                _index = -1;
            }

            public ValidationEntry Current => _entry;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _index++;
                if (!_enumerator.MoveNext())
                {
                    return false;
                }

                var key = ModelNames.CreateIndexModelName(_key, _index);
                var model = _enumerator.Current;

                if(_metadataList != null)
                {
                    _entry = new ValidationEntry(_metadataList[_index], key, model);
                }
                else
                {
                    _entry = new ValidationEntry(_metadata, key, model);
                }

                return true;
            }

            public void Dispose()
            {
            }

            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}
