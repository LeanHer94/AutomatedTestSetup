using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AutomatedTestSetup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Add Moq, XUnit and FluentAssertions usings
            var code = "" +
                "using Moq;\n" +
                "using Xunit;\n" +
                "using FluentAssertions;\n";

            var testClassName = string.Empty;
            var className = string.Empty;
            var injections = new Dictionary<string, string>();

            for (int i = 0; i < txtCode.Lines.Length; i++)
            {
                var line = txtCode.Lines[i].Trim();

                if (line.Contains("namespace"))
                {
                    code += "using " + line.Remove(0, 9).Trim() + ";\n\n";
                }

                //the class to test
                if (line.Contains("class"))
                {
                    var interfaceStart = line.IndexOf(':');
                    line = line.Remove(interfaceStart).Trim();

                    //public class removed
                    className = line.Remove(0, 12).Trim();
                    testClassName = className + "Test";

                    code += line + "Test" + "\n";
                    continue;
                }

                //injections
                if (line.StartsWith("private readonly"))
                {
                    //private readonly removed
                    line = line.Remove(0, 16).Trim();

                    //separate interface from variable name
                    //removing semi colon
                    var injection = line.Remove(line.Length-1).Split(' ');

                    injection[0] = "Mock<" + injection[0] + ">";

                    //key = variable name
                    injections.Add(injection[1], injection[0]);
                    
                    code += "private readonly " + injection[0] + " " + injection[1] + ";\n";
                    continue;
                }

                //constructor reached
                if (line.Contains("public " + className))
                {
                    code += "private readonly " + className + " " + className.ToLower() + ";\n\n";

                    break;
                }

                //using or { } or namespace or ' '
                code += line + "\n";
            }

            //creation of test class constructor
            code += "public " + testClassName + "()\n";
            code += "{\n";

            foreach (var item in injections)
            {
                //initilize mock object
                code += "this." + item.Key + " = new " + item.Value + "();\n";
            }

            //create class to test
            code += "\nthis." + className.ToLower() + " = new " + className + "(\n";

            for (int i = 0; i < injections.Count; i++)
            {
                //inject mock object
                if(i < injections.Count - 1)
                {
                    code += "\tthis." + injections.ElementAt(i).Key + ".Object,\n";
                }
                else
                {
                    code += "\tthis." + injections.ElementAt(i).Key + ".Object);\n";
                }
            }

            //close constructor
            code += "}\n";

            //create example test
            code += "\n[Fact]\n";
            code += "public void Test()\n";
            code += "{\n";

            //close test, class and namespace
            code += "}\n}\n}\n";

            txtCode.Text = code;
            txtTestClassName.Text = testClassName;
        }
    }
}