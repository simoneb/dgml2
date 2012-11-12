using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using System.Xml.XPath;

namespace DGML2.Test
{
    public class Dgml2GraphMlTests
    {
	    private XDocument _input;
	    private XDocument _output;
	    private XPathNavigator _navigator;
	    private XmlNamespaceManager _outputNamespaceManager;
		private static readonly Regex NamespaceReplacement = new Regex("/(?!@)", RegexOptions.Compiled);

	    [TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			_input = XDocument.Load(GetType().Assembly.GetManifestResourceStream(GetType().Namespace + ".dgml.xml"));
			_output = Dgml2GraphMl.Convert(_input);
		    _navigator = _output.CreateNavigator();
		    _outputNamespaceManager = new XmlNamespaceManager(new NameTable());
			_outputNamespaceManager.AddNamespace("graphml", Dgml2GraphMl.GraphMlNamespace.NamespaceName);
		}

		[SetUp]
		public void Setup()
		{
			
		}

		[Test]
		public void OneRoot()
		{
			Assert.That(_output.Elements(Dgml2GraphMl.GraphMlNamespace + "graphml").Count(), Is.EqualTo(1));
		}

		[Test]
		public void OneGraph()
		{
			Assert.That(Elements("/graphml/graph").Count(), Is.EqualTo(1));
		}

		[Test]
		public void GraphEdgeDefaultAttributeIsDirected()
		{
			Assert.AreEqual("directed", Value("/graphml/graph/@edgedefault"));
		}

		[Test]
		public void Nodes()
		{
			Assert.AreEqual(1, Elements("/graphml/graph/node[@id='n1']").Count());
			Assert.AreEqual(1, Elements("/graphml/graph/node[@id='n2']").Count());
		}

		[Test]
		public void Edges()
		{
			Assert.AreEqual(1, Elements("/graphml/graph/edge[@source='n1' and @target='n2']").Count());			
		}

		[Test]
		public void NodeBackgroundAddsDataToNode()
		{
			var dataKeyBackground = Element("/graphml/graph/node[@id='n1']/data[@key='background']");
			Assert.IsNotNull(dataKeyBackground);
			Assert.AreEqual("Yellow", dataKeyBackground.Value);
		}

	    [Test]
		public void NodeBackgroundMapsToKey()
	    {
		    var backgroundKey = Element("/graphml/graph/key[@id='background']");

		    Assert.IsNotNull(backgroundKey);

			Assert.AreEqual("node", backgroundKey.Attribute("for").Value);
			Assert.AreEqual("color", backgroundKey.Attribute("attr.name").Value);
			Assert.AreEqual("string", backgroundKey.Attribute("attr.type").Value);
	    }

	    #region Helpers

		private string Value(string path)
	    {
		    var iterator = _navigator.Select(NamespaceReplacement.Replace(path, "/graphml:"), _outputNamespaceManager);
		    iterator.MoveNext();
		    return iterator.Current.Value;
	    }

	    private XElement Element(string path)
		{
			return Elements(path).FirstOrDefault();
		}

	    private IEnumerable<XElement> Elements(string path)
	    {
			return _output.XPathSelectElements(NamespaceReplacement.Replace(path, "/graphml:"), _outputNamespaceManager);
		}

		#endregion
	}
}