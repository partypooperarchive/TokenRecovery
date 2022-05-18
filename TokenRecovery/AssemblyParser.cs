/*
 * Created by SharpDevelop.
 * User: User
 * Date: 29.06.2021
 * Time: 20:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace TokenRecovery
{
	/// <summary>
	/// Description of AssemblyParser.
	/// </summary>
	public class AssemblyParser
	{
		private AssemblyDefinition assembly = null;
		
		private string base_class_name = "Ololo";
		private string cmd_id_field_name = "Azaza";
		private const string token_attrib_name = "TokenAttribute";
		
		private SortedDictionary<uint, KeyValuePair<int, string>> types_dict = null;
		
		public AssemblyParser(string filename, string base_class_name, string cmd_id_field_name)
		{
			assembly = AssemblyDefinition.ReadAssembly(filename);
			this.base_class_name = base_class_name;
			this.cmd_id_field_name = cmd_id_field_name;
		}
		
		public void parse()
		{
			var types = GetProtobufTypes();
			
			types_dict = new SortedDictionary<uint, KeyValuePair<int, string>>();
			
			foreach (var type in types)
			{
				var cmd_id = GetCmdId(type);
				var token = GetToken(type);
				types_dict.Add(token, new KeyValuePair<int, string>(cmd_id, type.Name));
			}
		}
		
		public void writeToCSV(string filename)
		{
			var csv = new StringBuilder();
			
			foreach (var pair in types_dict)
			{
				uint token = pair.Key;
				int cmd_id = pair.Value.Key;
				string name = pair.Value.Value;
				var line = string.Format("{0},{1},{2}", name, token, cmd_id);
				csv.AppendLine(line);
			}
			
			File.WriteAllText(filename, csv.ToString());
		}
		
		private TypeDefinition[] GetTypes()
		{
			return assembly.MainModule.Types.ToArray();
		}
		
		private TypeDefinition[] GetProtobufTypes()
		{
			return GetTypes().OrderBy(t => t.Name).Where(t => IsProtobuf(t)).ToArray();
		}
		
		private bool IsProtobuf(TypeDefinition t)
		{
			//this was the case for 1.4.51, but sadly not anymore
			//return t.FullName.StartsWith("Proto.") && !t.FullName.Contains("+");
			
			var base_type = t.BaseType;
			
			if (base_type == null || base_type.Name != base_class_name)
				return false;
			
			// There should exist nested type with nested enum with element "CmdId"
			foreach (var nested_type in t.NestedTypes)
			{
				foreach (var inner_type in nested_type.NestedTypes)
				{
					if (inner_type.IsEnum) 
					{
						foreach (var field in inner_type.Fields)
						{
							if (field.Name == cmd_id_field_name)
								return true;
						}
					}
				}
			}
			
			return false;
		}
		
		private int GetCmdId(TypeDefinition t)
		{
			foreach (var nested_type in t.NestedTypes)
			{
				foreach (var inner_type in nested_type.NestedTypes)
				{
					if (inner_type.IsEnum) 
					{
						foreach (var field in inner_type.Fields)
						{
							if (field.Name == cmd_id_field_name)
								return int.Parse(field.Constant.ToString());
						}
					}
				}
			}
			
			throw new ArgumentException();
		}
		
		private uint GetToken(TypeDefinition t)
		{
			foreach (var attrib in t.CustomAttributes)
			{
				if (attrib.AttributeType.Name == token_attrib_name)
				{
					var token = attrib.Fields[0].Argument.Value.ToString();
					return Convert.ToUInt32(token, 16);
				}
			}
			
			throw new ArgumentException();
		}
	}
}
