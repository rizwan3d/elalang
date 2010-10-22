using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Reflection;
using Ela.Linking;
using System.Xml.XPath;
using System.IO;
using Ela.Runtime;

namespace Ela.ModuleGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			var asm = Assembly.LoadFrom(args[0]);
			var outFile = args[1];
			var attrs = asm.GetCustomAttributes(typeof(ElaModuleAttribute), false);

			var sb = new StringBuilder();
			var xw = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true });
			xw.WriteStartElement("modules");

			foreach (ElaModuleAttribute a in attrs)
			{
				xw.WriteStartElement("module");
				xw.WriteAttributeString("name", a.ModuleName);
				xw.WriteAttributeString("namespace", a.ClassType.Namespace);
				xw.WriteAttributeString("className", a.ClassType.Name);
				var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

				foreach (var mi in a.ClassType.GetMethods(flags))
				{
					var attr = (FunctionAttribute)Attribute.GetCustomAttribute(mi, typeof(FunctionAttribute));

					if (attr != null)
					{
						xw.WriteStartElement("function");
						xw.WriteAttributeString("name", attr.Name);
						xw.WriteAttributeString("returnType", mi.ReturnType.Name);
						xw.WriteAttributeString("methodName", mi.Name);
						xw.WriteAttributeString("unlimitedPars", attr.UnlimitedParameters.ToString());

						var pars = mi.GetParameters();

						if (attr.UnlimitedParameters && (pars.Length != 1 && pars[0].ParameterType != typeof(RuntimeValue[])))
							throw new Exception("Functions with unlimited parameters should have a single parameter of type RuntimeValue[].");
						else
						{
							foreach (var pi in pars)
							{
								xw.WriteStartElement("param");
								xw.WriteAttributeString("type", pi.ParameterType.Name);
								xw.WriteEndElement();
							}
						}

						xw.WriteEndElement();
					}
				}

				xw.WriteEndElement();
			}

			xw.WriteEndElement();
			xw.Flush();
			
			var xsl = new XslCompiledTransform(true);
			
			using (var xr = XmlReader.Create(typeof(Program).Assembly.
				GetManifestResourceStream("Ela.ModuleGenerator.CodeGenerator.xslt")))
			{
				xsl.Load(xr);
				
				using (var s = File.OpenWrite(outFile))
					xsl.Transform(new XPathDocument(new StringReader(sb.ToString())), new XsltArgumentList(), s);
			}
		}
	}
}
