using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiFpgaGen
{
    public class SourceCodeStringBuilder
    {
        private StringBuilder builder = new StringBuilder();
        private string currentIndent;
        private string stepsPerIndent;

        public SourceCodeStringBuilder(int stepsPerIndent)
        {
            this.stepsPerIndent = "";
            for (int i = 0; i < stepsPerIndent; i++)
            {
                this.stepsPerIndent += " ";
            }
            this.currentIndent = "";
        }

        public SourceCodeStringBuilder Clear()
        {
            builder.Clear();
            return this;
        }

        public SourceCodeStringBuilder AppendIndent()
        {
            builder.Append(currentIndent);
            return this;
        }

        public SourceCodeStringBuilder Append(string value)
        {
            builder.Append(value);
            return this;
        }

        public SourceCodeStringBuilder AppendLine(string value)
        {
            builder.AppendLine(value);
            return this;
        }

        public SourceCodeStringBuilder AppendLine()
        {
            builder.AppendLine();
            return this;
        }

        public SourceCodeStringBuilder AppendNewLines(int numberOfNewLines)
        {

            for (int i = 0; i < numberOfNewLines; i++)
            {
                builder.AppendLine();
            }
            return this;
        } 

        public SourceCodeStringBuilder AddIndent()
        {
            currentIndent += stepsPerIndent;
            return this;
        }

        public SourceCodeStringBuilder RemoveIndent()
        {
            currentIndent = currentIndent[0..^stepsPerIndent.Length];
            return this;
        }

        public int Length => builder.Length;

        public SourceCodeStringBuilder Insert(int index, string value)
        {
            builder.Insert(index, value);
            return this;
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
