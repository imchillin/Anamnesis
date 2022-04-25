// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Extensions;

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Documents;
using System.Xaml;
using Markdig;
using Markdig.Wpf;

using XamlReader = System.Windows.Markup.XamlReader;

public static class Markdown
{
	public static FlowDocument ToDocument(string markdown)
	{
		string? xaml = Markdig.Wpf.Markdown.ToXaml(markdown, new MarkdownPipelineBuilder().UseSupportedExtensions().Build());

		using MemoryStream? stream = new MemoryStream(Encoding.UTF8.GetBytes(xaml));
		using XamlXmlReader? reader = new XamlXmlReader(stream, new MyXamlSchemaContext());

		object? document = XamlReader.Load(reader);

		if (document is FlowDocument flowDoc)
		{
			return flowDoc;
		}

		throw new Exception($"document: {document} was not a flow document");
	}

	private class MyXamlSchemaContext : XamlSchemaContext
	{
		public override bool TryGetCompatibleXamlNamespace(string xamlNamespace, out string compatibleNamespace)
		{
			if (xamlNamespace.Equals("clr-namespace:Markdig.Wpf", StringComparison.Ordinal))
			{
				compatibleNamespace = $"clr-namespace:Markdig.Wpf;assembly={Assembly.GetAssembly(typeof(Markdig.Wpf.Styles))?.FullName}";
				return true;
			}

			return base.TryGetCompatibleXamlNamespace(xamlNamespace, out compatibleNamespace);
		}
	}
}
