using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.SimAntics.Engine;

namespace TSO.Simantics.engine
{
    public class VMSimanticsException : Exception
    {
        private string message;
        private VMStackFrame context;
        public VMSimanticsException(string message, VMStackFrame context) : base(message)
        {
            this.context = context;
            this.message = message;
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(message);
            output.AppendLine();
            output.AppendLine();
            output.Append(GetStackTrace());
            return output.ToString();
        }

        public string GetStackTrace()
        {
            if (context == null) return "No Stack Info.";
            StringBuilder output = new StringBuilder();
            
            var stack = context.Thread.Stack.GetEnumerator();
            while (stack.Current != null)
            {
                var frame = stack.Current;
                //run in tree:76

                output.Append('(');
                output.Append(frame.Caller.ToString());
                output.Append(':');
                output.Append(frame.Callee.ToString());
                output.Append(") ");

                if (frame is VMRoutingFrame)
                {
                    output.Append("VMPathFinder to: ");
                    output.Append(((VMRoutingFrame)frame).Callee.Position.ToString());
                }
                else 
                {
                    output.Append(frame.Routine.Rti.Name);
                    output.Append(':');
                    output.Append(frame.InstructionPointer);
                }
                output.AppendLine();

                stack.MoveNext();
            }

            return output.ToString();
        }
    }
}
