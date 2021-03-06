﻿using System;
using Mond.Binding;

namespace Mond.Libraries.Core
{
    [MondModule("")]
    internal class ErrorModule
    {
        [MondFunction("error")]
        public static void Error(string message)
        {
            throw new MondRuntimeException(message);
        }

        [MondFunction("try")]
        public static MondValue Try(MondState state, MondValue function, params MondValue[] arguments)
        {
            if (function.Type != MondValueType.Function)
                throw new MondRuntimeException("try: first argument must be a function");

            var obj = new MondValue(MondValueType.Object);

            try
            {
                var result = state.Call(function, arguments);
                obj["result"] = result;
            }
            catch (Exception e)
            {
                obj["error"] = e.Message;
            }

            return obj;
        }
    }
}
