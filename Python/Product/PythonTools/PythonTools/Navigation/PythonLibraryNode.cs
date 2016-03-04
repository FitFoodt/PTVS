// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.PythonTools.Intellisense;
using Microsoft.PythonTools.Interpreter;
using Microsoft.PythonTools.Parsing.Ast;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Navigation;

namespace Microsoft.PythonTools.Navigation {
    internal class PythonLibraryNode : CommonLibraryNode {
        private readonly CompletionResult _value;

        public PythonLibraryNode(LibraryNode parent, CompletionResult value, IVsHierarchy hierarchy, uint itemId, IList<LibraryNode> children)
            : base(parent, value.Name, value.Name, hierarchy, itemId, GetLibraryNodeType(value, parent), children: children) {
            _value = value;
        }

        private static LibraryNodeType GetLibraryNodeType(CompletionResult value, LibraryNode parent) {
            switch (value.MemberType) {
                case PythonMemberType.Class:
                    return LibraryNodeType.Classes;
                case PythonMemberType.Function:
                    if (parent is PythonFileLibraryNode) {
                        return LibraryNodeType.Classes;
                    }
                    return LibraryNodeType.Members;
                default:
                    return LibraryNodeType.Members;
            }
        }
        protected PythonLibraryNode(PythonLibraryNode node) : base(node) {
            _value = node._value;
        }

        protected PythonLibraryNode(PythonLibraryNode node, string newFullName) : base(node, newFullName) {
            _value = node._value;
        }

        public override LibraryNode Clone() {
            return new PythonLibraryNode(this);
        }

        public override LibraryNode Clone(string newFullName) {
            return new PythonLibraryNode(this, newFullName);
        }

        public override StandardGlyphGroup GlyphType {
            get {
                switch (_value.MemberType) {
                    case PythonMemberType.Class:
                        return StandardGlyphGroup.GlyphGroupClass;
                    case PythonMemberType.Method:
                    case PythonMemberType.Function:
                        return StandardGlyphGroup.GlyphGroupMethod;
                    case PythonMemberType.Field:
                    case PythonMemberType.Instance:
                        return StandardGlyphGroup.GlyphGroupField;
                    case PythonMemberType.Constant:
                        return StandardGlyphGroup.GlyphGroupConstant;
                    case PythonMemberType.Module:
                        return StandardGlyphGroup.GlyphGroupModule;
                    default:
                        return StandardGlyphGroup.GlyphGroupUnknown;
                }
            }
        }

        public override string GetTextRepresentation(VSTREETEXTOPTIONS options) {
            StringBuilder res = new StringBuilder();
            foreach (var value in _value.Values) {
                bool isAlias = false;
                foreach (var desc in value.description) {
                    if (desc.kind == "name") {
                        if (desc.text != Name) {
                            isAlias = true;
                        }
                    }
                }

                if (isAlias) {
                    res.Append(Name);
                    res.Append(" (alias of ");
                }

                foreach (var desc in value.description) {
                    if (desc.kind == "enddecl") {
                        break;
                    }
                    res.Append(desc.text);
                }

                if (isAlias) {
                    res.Append(")");
                }
            }

            if (res.Length == 0) {
                return Name;
            }
            return res.ToString();
        }

        public override void FillDescription(_VSOBJDESCOPTIONS flags, IVsObjectBrowserDescription3 description) {
            description.ClearDescriptionText();
            foreach (var value in _value.Values) {
                foreach (var desc in value.description) {
                    VSOBDESCRIPTIONSECTION kind;
                    switch (desc.kind) {
                        case "enddecl": kind = VSOBDESCRIPTIONSECTION.OBDS_ENDDECL; break;
                        case "name": kind = VSOBDESCRIPTIONSECTION.OBDS_NAME; break;
                        case "param": kind = VSOBDESCRIPTIONSECTION.OBDS_PARAM; break;
                        case "comma": kind = VSOBDESCRIPTIONSECTION.OBDS_COMMA; break;
                        default: kind = VSOBDESCRIPTIONSECTION.OBDS_MISC; break;

                    }
                    description.AddDescriptionText3(desc.text, kind, null);
                }
            }
        }


        public override int GetLibGuid(out Guid pGuid) {
            pGuid = new Guid(CommonConstants.LibraryGuid);
            return VSConstants.S_OK;
        }
    }
}
