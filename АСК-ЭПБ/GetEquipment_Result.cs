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
    
    public partial class GetEquipment_Result
    {
        public int EQUIPMENTID { get; set; }
        public string NAME_EQUIPMENT { get; set; }
        public string TECHNICAL_DOCUMENTATION { get; set; }
        public string EquipmentState { get; set; }
        public string EquipmentActivity { get; set; }
        public int EQUIPMENT_STATEID { get; set; }
        public int EQUIPMENT_ACTIVITYID { get; set; }
        public Nullable<System.DateTime> LAST_DATE_CHECKS { get; set; }
    }
}
