//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyOnlineExam
{
    using System;
    using System.Collections.Generic;
    
    public partial class Choice
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Choice()
        {
            this.TestXPaper = new HashSet<TestXPaper>();
        }
    
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Label { get; set; }
        public decimal Points { get; set; }
        public bool IsActive { get; set; }
    
        public virtual Question Question { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TestXPaper> TestXPaper { get; set; }
    }
}
