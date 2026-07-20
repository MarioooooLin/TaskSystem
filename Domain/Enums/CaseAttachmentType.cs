namespace Domain.Enums;

/// <summary>
/// 案件附件類型。
/// 1=ReferenceMaterial 參考素材
/// 2=Script 腳本
/// 3=Contract 合約草本
/// 4=Other 其他
/// </summary>
public enum CaseAttachmentType : short
{
    ReferenceMaterial = 1,
    Script = 2,
    Contract = 3,
    Other = 4
}
