-- ================================================================
-- 為既有 Submissions 表新增 RejectReason 欄位
-- 若已存在則略過
-- ================================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'Submissions')
        AND name = N'RejectReason'
) BEGIN
ALTER TABLE Submissions
ADD RejectReason NVARCHAR(500) NULL;
PRINT N'Submissions.RejectReason 欄位新增完成。';
END
ELSE BEGIN PRINT N'Submissions.RejectReason 欄位已存在，略過。';
END