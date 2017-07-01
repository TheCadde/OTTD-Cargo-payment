using System;

namespace OTTD_Cargo_Payment.Annotations {
    internal class DynamicBindingAttribute : Attribute {
        public string Comment { get; }

        public string Group { get; }

        public string DisplayName { get; }

        public readonly int GroupSortOrder;
        public readonly int ItemSortOrder;

        public DynamicBindingAttribute(string group, string displayName, string comment, int gropSortOrder, int itemSortOrder) {
            Comment = comment;
            Group = group;
            DisplayName = displayName;
            GroupSortOrder = gropSortOrder;
            ItemSortOrder = itemSortOrder;
        }
    }
}
