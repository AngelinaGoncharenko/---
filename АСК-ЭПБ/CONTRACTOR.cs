//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace АСК_ЭПБ
{
    using System;
    using System.Collections.Generic;
    
    public partial class CONTRACTOR
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CONTRACTOR()
        {
            this.WORK_REQUEST = new HashSet<WORK_REQUEST>();
        }
    
        public int CONTRACTORID { get; set; }
        public string NAME_CONTRACTOR { get; set; }
        public string SUBCONTRACT { get; set; }
        public string CONTACTS { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WORK_REQUEST> WORK_REQUEST { get; set; }
    }
}
