using Application.Ports;
using Quartz;

namespace Application.Jobs;

public class ClearUnfinishedUploadRecordsJob(
    ILogger<ClearUnfinishedUploadRecordsJob> logger,
    IRecordPort recordPort,
    IFilePort filePort
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogDebug("Starting to clear unfinished upload records...");

        var records = await recordPort.GetAll();

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            if (!record.HasExpired()) continue;

            logger.LogInformation("Record with UploadId {UploadId} has expired. Trying to abort and delete.",
                record.UploadId);

            try
            {
                await filePort.AbortUpload(record.RemoteUploadId, record.ObjectKey);
                logger.LogInformation("Aborted upload for Record with UploadId {UploadId}.", record.UploadId);
            }
            catch (Exception e)
            {
                logger.LogError(e,
                    "Failed to abort upload for Record with UploadId {UploadId}. Skipping to next record.",
                    record.UploadId);
                continue;
            }

            try
            {
                await recordPort.DeleteByUploadId(record.UploadId);
                logger.LogInformation("Deleted Record with UploadId {UploadId}.", record.UploadId);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete Record with UploadId {UploadId}. Skipping to next record.",
                    record.UploadId);
            }
        }

        logger.LogDebug("Finished clearing unfinished upload records...");
    }
}