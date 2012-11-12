using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Linq;

namespace DGML2
{
	class Program
	{
		static void Main(string[] args)
		{
			var input = args[0];
			var output = args.Length >= 2 ? args[1] : Path.ChangeExtension(input, "graphml");

			Dgml2GraphMl.Convert(XDocument.Load(input)).Save(output);

			Console.WriteLine("Output saved to {0}", output);
		}
	}

	public static class Dgml2GraphMl
	{
		public static XDocument Convert(XDocument input)
		{
			IList<XElement> keys = new List<XElement>();

			XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
			XNamespace schemaLocation = "http://graphml.graphdrawing.org/xmlns http://graphml.graphdrawing.org/xmlns/1.0/graphml.xsd";

			var output =  new XDocument(
				Element("graphml",
					Attribute(XNamespace.Xmlns + "xsi", xsi),
					Attribute(xsi + "schemaLocation", schemaLocation),

					from grap in Elements(input, "DirectedGraph") 
					let nodes = Element(grap, "Nodes") where nodes != null
					let links = Element(grap, "Links") where links != null
					select Element("graph",
						Attribute("edgedefault", "directed"),
		
						from node in Elements(nodes, "Node")
						let background = NodeBackground(Attribute(node, "Background"), keys)
						let id = node.Attribute("Id") where id != null
						select Element("node",
							Attribute("id", id.Value),
							MaybeElement(background, "data",
								Attribute("key", background.Value.Key),
								background.Value.Value)),
						from link in Elements(links, "Link")
						let source = link.Attribute("Source") where source != null
						let target = link.Attribute("Target") where target != null
						select Element("edge",
							Attribute("source", source.Value),
							Attribute("target", target.Value)
						)
					)
				)
			);

			output.Element(GraphMlNamespace + "graphml").Element(GraphMlNamespace + "graph")
				.AddFirst(keys);

			return output;
		}

		private static KeyValuePair<string, object>? NodeBackground(XAttribute background, IList<XElement> keys)
		{
			if (background == null || string.IsNullOrEmpty(background.Value))
				return null;

			var color = HandleColor(background.Value);

			if (string.IsNullOrEmpty(color))
				return null;

			const string id = "background";

			keys.Add(Element("key",
			                  Attribute("id", id),
			                  Attribute("for", "node"),
			                  Attribute("attr.name", "color"),
			                  Attribute("attr.type", "string")));

			return new KeyValuePair<string, object>(id, color);
		}

		private static string HandleColor(string dgmlColor)
		{
			var found = typeof (Colors).GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty)
				.FirstOrDefault(p => dgmlColor.Equals(p.Name, StringComparison.OrdinalIgnoreCase));

			return found != null ? found.Name.ToLowerInvariant() : string.Empty;
		}

		private static XAttribute Attribute(XName name, object value)
		{
			return new XAttribute(name, value);
		}

		private static XAttribute Attribute(XElement element, XName name)
		{
			return element.Attribute(name);
		}

		private static XElement Element(string name, params object[] content)
		{
			return new XElement(GraphMlNamespace + name, content);
		}

		private static IEnumerable<XElement> MaybeElement<T>(T? instance, string name, params object[] content) where T : struct
		{
			if(instance.HasValue)
				yield return new XElement(GraphMlNamespace + name, content);
		}

		private static XElement Element(XContainer element, string name)
		{
			return element.Element(DgmlNamespace + name);
		}

		private static IEnumerable<XElement> Elements(XContainer input, string name)
		{
			return input.Elements(DgmlNamespace + name);
		}

		public static XNamespace DgmlNamespace
		{
			get { return "http://schemas.microsoft.com/vs/2009/dgml"; }
		}

		public static XNamespace GraphMlNamespace
		{
			get { return "http://graphml.graphdrawing.org/xmlns"; }
		}
	}
}
