// CodeDOM exercise - (Written in 2016, posted: 2018)
// Does three things
// 1) Autogenerates code containing a function that adds 2 parameters (integers) "add2integers.cs"
// 2) Compiles the code into a dll file "libraryadd2ints.dll"
// 3) Uses built-in functionality of the compiled dll in part 2) to add 2 numbers
// This is a simple program that utilizes various functionalities of CodeDOM to execute a certain task

using System;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;

namespace CodedomAssgnment
{
    class Program
    {
        static void Main(string[] args)
        {
			/*
			PART 1
			*/
			
            // setup new object graph instance
            CodeCompileUnit cc_unit_obj = new CodeCompileUnit();
            CodeNamespace cn_obj = new CodeNamespace("ProgramNamespace");

            // import system
            cn_obj.Imports.Add(new CodeNamespaceImport("System"));
            cc_unit_obj.Namespaces.Add(cn_obj);

            // create class in library source code
            CodeTypeDeclaration ctd_class_obj = new CodeTypeDeclaration("ProgramClass");
            cn_obj.Types.Add(ctd_class_obj);

            // create method in library source code
            CodeMemberMethod cmm_obj = new CodeMemberMethod();
            cmm_obj.Name = "add2Ints";

            // set return type of method
            cmm_obj.ReturnType = new CodeTypeReference("System.Int32");
            cmm_obj.Attributes = MemberAttributes.Public;

            // set parameters
            CodeParameterDeclarationExpression p1 = new CodeParameterDeclarationExpression("System.Int32", "a_int_param");
            cmm_obj.Parameters.Add(p1);

            CodeParameterDeclarationExpression p2 = new CodeParameterDeclarationExpression("System.Int32", "b_int_param");
            cmm_obj.Parameters.Add(p2);

            // set function body
            cmm_obj.Statements.Add(new CodeSnippetStatement("return (a_int_param+b_int_param);"));

            ctd_class_obj.Members.Add(cmm_obj);

            // needed to save to source file (and create dll file)
            CSharpCodeProvider provider = new CSharpCodeProvider();

            String output_file_name = "add2Integers.cs";

            // save to file (from object graph)
            //https://msdn.microsoft.com/en-us/library/saf5ce06(v=vs.110).aspx
            using (StreamWriter sw = new StreamWriter(output_file_name, false))
            {
                IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");

                provider.GenerateCodeFromCompileUnit(cc_unit_obj, tw, new CodeGeneratorOptions());

                tw.Close();
            }

			/*
			PART 2
			*/
			
            // ..as before
            CSharpCodeProvider lib_provider = new CSharpCodeProvider();

            CompilerParameters cparams = new CompilerParameters();

            cparams.ReferencedAssemblies.Add("System.dll");

            // we want a library
            cparams.GenerateExecutable = false;
            cparams.OutputAssembly = "libraryadd2ints.dll";
            cparams.GenerateInMemory = false;

            // Invoke compilation.
            CompilerResults comp_results = provider.CompileAssemblyFromFile(cparams, output_file_name);

            if (comp_results.Errors.Count > 0)
            {
                Console.WriteLine("Build errors detected...");
                Console.ReadLine();
                return;
            }
            else
            {
                Console.WriteLine("Source built...", output_file_name, comp_results.PathToAssembly);
            }

            // extract info from dll file...
			
			/*
			PART 3
			*/

            // http://stackoverflow.com/questions/15653921/get-current-folder-path
            String current_dir_plus_dll_file_str = System.IO.Directory.GetCurrentDirectory() + "\\libraryadd2ints.dll";

            // http://stackoverflow.com/questions/18362368/loading-dlls-at-runtime-in-c-sharp
            Assembly dll_var = Assembly.LoadFile(current_dir_plus_dll_file_str);

            Type class_type = dll_var.GetType("ProgramNamespace.ProgramClass");

            object v = new object();

            v = Activator.CreateInstance(class_type);

            var method_output = class_type.GetMethod("add2Ints");

            Console.WriteLine("Please enter Integer parameter 1:");

            System.Int32 p1_input = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Please enter Integer parameter 2:");

            System.Int32 p2_input = Convert.ToInt32(Console.ReadLine());

            // result should be p1_input + p2_input
            object[] params_passed = new object[] { p1_input, p2_input };

            Console.WriteLine("Final result extracted from dll: {0}", method_output.Invoke(v, params_passed));

            Console.ReadLine();
        }
    }
}
