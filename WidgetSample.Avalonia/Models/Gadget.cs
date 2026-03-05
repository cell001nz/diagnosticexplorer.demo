
#region Copyright
// Diagnostic Explorer, a .Net diagnostic toolset
// Copyright (C) 2010 Cameron Elliot
// 
// This file is part of Diagnostic Explorer.
// 
// Diagnostic Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Diagnostic Explorer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with Diagnostic Explorer.  If not, see <http://www.gnu.org/licenses/>.
// 
// http://diagexplorer.sourceforge.net/
#endregion
using System;
using DiagnosticExplorer;
using Metalama.Patterns.Observability;
namespace WidgetSample.Avalonia.Models;

// [Observable]
public partial class Gadget
{
    private static readonly string[] _names = ["Gadget X", "Gadget Y", "Gadget Z", "Gadget W"];
    private static readonly string[] _purposes = ["Technical", "Muckabout", "Stuff"];
    public Gadget(int id)
    {
        Id = id;
        Name = GetRandom(_names);
        Purpose = GetRandom(_purposes);
    }
    
    public int Id { get; }

    public string? Name { get; set; }

    public string? Purpose { get; set; }

    [DiagnosticMethod]
    public void Randomise()
    {
        Name = GetRandom(_names);
        Purpose = GetRandom(_purposes);
    }
    [DiagnosticMethod]
    public void Clear()
    {
        Name = null;
        Purpose = null;
    }
    
    private string GetRandom(string[] items) => items[Random.Shared.Next(0, items.Length)];
    public override string ToString() => $"Gadget {Id}";
}
