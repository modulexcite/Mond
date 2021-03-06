﻿#if NO_EXPRESSIONS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if !UNITY
using System.Runtime.ExceptionServices;
#endif

namespace Mond.Binding
{
    public static partial class MondFunctionBinder
    {
        private static MondFunction BindImpl(string moduleName, MethodTable method, string nameOverride = null)
        {
            var errorPrefix = BindingError.ErrorPrefix(moduleName, nameOverride ?? method.Name);

            return (state, args) =>
            {
                MethodBase function;
                Func<object, MondValue> returnConversion;
                var parameters = BuildParameterArray(errorPrefix, method, state, null, args, out function, out returnConversion);

                return returnConversion(Call(() => function.Invoke(null, parameters)));
            };
        }

        private static MondInstanceFunction BindInstanceImpl(string moduleName, MethodTable method, string nameOverride = null, bool fakeInstance = false)
        {
            var errorPrefix = BindingError.ErrorPrefix(moduleName, nameOverride ?? method.Name);

            if (!fakeInstance)
            {
                return (state, instance, args) =>
                {
                    MethodBase function;
                    Func<object, MondValue> returnConversion;
                    var parameters = BuildParameterArray(errorPrefix, method, state, instance, args, out function, out returnConversion);

                    var classInstance = instance.UserData;
                    return returnConversion(Call(() => function.Invoke(classInstance, parameters)));
                };
            }

            return (state, instance, args) =>
            {
                MethodBase function;
                Func<object, MondValue> returnConversion;
                var parameters = BuildParameterArray(errorPrefix, method, state, instance, args, out function, out returnConversion);

                return returnConversion(Call(() => function.Invoke(null, parameters)));
            };
        }

        private static MondConstructor BindConstructorImpl(string moduleName, MethodTable method)
        {
            var errorPrefix = BindingError.ErrorPrefix(moduleName, "#ctor");
            
            return (state, instance, args) =>
            {
                MethodBase constructor;
                Func<object, MondValue> returnConversion;
                var parameters = BuildParameterArray(errorPrefix, method, state, instance, args, out constructor, out returnConversion);

                return Call(() => ((ConstructorInfo)constructor).Invoke(parameters));
            };
        }

        private static T Call<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException == null)
                    throw;

#if !UNITY
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
#endif

                throw e.InnerException; // shouldnt reach this
            }
        }

        private static object[] BuildParameterArray(
            string errorPrefix,
            MethodTable methodTable,
            MondState state,
            MondValue instance,
            MondValue[] args,
            out MethodBase methodBase,
            out Func<object, MondValue> returnConversion)
        {
            Method method = null;

            if (args.Length < methodTable.Methods.Count)
                method = FindMatch(methodTable.Methods[args.Length], args);

            if (method == null)
                method = FindMatch(methodTable.ParamsMethods, args);

            if (method != null)
            {
                methodBase = method.Info;
                returnConversion = method.ReturnConversion;

                var parameters = method.Parameters;
                var result = new object[parameters.Count];

                var j = 0;
                for (var i = 0; i < result.Length; i++)
                {
                    var param = parameters[i];

                    switch (param.Type)
                    {
                        case ParameterType.Value:
                            if (j < args.Length)
                                result[i] = param.Conversion(args[j++]);
                            else
                                result[i] = param.Info.DefaultValue;
                            break;

                        case ParameterType.Params:
                            result[i] = Slice(args, method.MondParameterCount);
                            break;

                        case ParameterType.Instance:
                            result[i] = instance;
                            break;

                        case ParameterType.State:
                            result[i] = state;
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }

                return result;
            }

            throw new MondBindingException(BindingError.ParameterTypeError(errorPrefix, methodTable));
        }

        private static Method FindMatch(List<Method> methods, MondValue[] args)
        {
            for (var i = 0; i < methods.Count; i++)
            {
                var method = methods[i];
                var parameters = method.ValueParameters;

                if (method.RequiredMondParameterCount == 0 && (args.Length == 0 || method.HasParams))
                    return methods[i];

                for (var j = 0; j < parameters.Count; j++)
                {
                    var param = parameters[j];

                    if (!(param.IsOptional && j >= args.Length) && param.MondTypes[0] != MondValueType.Undefined)
                    {
                        var arg = args[j];

                        if (!param.MondTypes.Contains(arg.Type))
                            break;

                        if (param.UserDataType != null && !param.UserDataType.IsInstanceOfType(arg.UserData))
                            break;
                    }

                    if (j == parameters.Count - 1)
                        return method;
                }
            }

            return null;
        }

        internal static Func<MondValue, object> MakeParameterConversion(Type parameterType)
        {
            if (parameterType == typeof(string))
                return v => (string)v;

            if (parameterType == typeof(bool))
                return v => (bool)v;

            if (parameterType == typeof(double))
                return v => (double)v;

            // cant use the Convert class for the rest of these...

            if (parameterType == typeof(float))
                return v => (float)v;

            if (parameterType == typeof(int))
                return v => (int)v;

            if (parameterType == typeof(uint))
                return v => (uint)v;

            if (parameterType == typeof(short))
                return v => (short)v;

            if (parameterType == typeof(ushort))
                return v => (ushort)v;

            if (parameterType == typeof(sbyte))
                return v => (sbyte)v;

            if (parameterType == typeof(byte))
                return v => (byte)v;

            return null;
        }

        internal static Func<object, MondValue> MakeReturnConversion(Type returnType)
        {
            if (returnType == typeof(void))
                return o => MondValue.Undefined;

            if (returnType == typeof(MondValue))
                return o => ReferenceEquals(o, null) ? MondValue.Null : (MondValue)o;

            if (returnType == typeof(string))
                return o => ReferenceEquals(o, null) ? MondValue.Null : (MondValue)(string)o;

            if (returnType == typeof(bool))
                return o => (bool)o;

            if (NumberTypes.Contains(returnType))
                return o => Convert.ToDouble(o);

            var classAttrib = returnType.Attribute<MondClassAttribute>();
            if (classAttrib != null && classAttrib.AllowReturn)
            {
                MondValue prototype;
                MondClassBinder.Bind(returnType, out prototype);

                return o =>
                {
                    var obj = new MondValue(MondValueType.Object);
                    obj.Prototype = prototype;
                    obj.UserData = o;
                    return obj;
                };
            }

            throw new MondBindingException(BindingError.UnsupportedReturnType, returnType);
        }
    }
}

#endif
