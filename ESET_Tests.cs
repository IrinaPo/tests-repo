using System;

public class ESET_Tests
{
    private var BasePath = "c:\\eset.installs\\LC\\2.0.6.0\\";
    private var IncorrectPath = "123";
    private var BaseFileName = "lc_logs.zip";
    private var IncorrectFileName = "lc_logs";
    private var LongFileName = "pcRsAQSCEmLX4eCJpOoWmnSGIWSBMQYfIthkUsP1QSx13QphToR9X" +
                               "DEA5Andn0zdk4r9QnOu2cEy9MnGefEionmFh6BhLqoJU9nEVMEPylwg" +
                               "IKfYHBJyepjnriV8Fh2WrNYWfMP7ALE7r8vTXaPPPEonhHBa8N4sdywe" +
                               "e6z6ST6CVKnDkYnqf523XCWTxtLfEPJ73QwMmZKJ8VvKL7wjM0YxqGyA1abCfzTfEz.zip";

    [Test]
    public void RunApplication_CheckDefaultStates()
    {
        // act
        var logCollectorForm = Application.Run();

        // assert
        foreach (var type in ArtifactsType)
            type.IsChecked.Wait().EqualTo(true);
        
        logCollectorForm.ArtifactsToCollectAll.IsChecked.Wait().EqualTo(true);
        logCollectorForm.LogsAgeLimit.Value.Wait().EqualTo(30);
        logCollectorForm.LogCollectionMode.Value.Wait().EqualTo(Filtered_binary);
        logCollectorForm.LogPathBlock.Text.Wait().EqualTo(BasePathConstant);
        logCollectorForm.CollectButton.Disabled.Wait().EqualTo(false);
    }

    [Test]
    public void NoArtifactsType_CollectButtonDisabled()
    {
        // act
        var logCollectorForm = Application.Run();
        logCollectorForm.ArtifactsToCollectAll.Uncheck();
        
        // assert
        logCollectorForm.CollectButton.Disabled.Wait().EqualTo(true);
        logCollectorForm.ArtifactsToCollect.Select(ArtifactsType.RunningProcesses);
        logCollectorForm.CollectButton.Disabled.Wait().EqualTo(false);
    }

    [Test]
    public void ArtifactsToCollectAll_WorkCorrect()
    {
        // act
        var logCollectorForm = Application.Run();
        logCollectorForm.ArtifactsToCollectAll.Uncheck();

        // assert
        foreach (var type in ArtifactsType)
            type.IsChecked.Wait().EqualTo(false);

        logCollectorForm.ArtifactsToCollect.Select(ArtifactsType.RunningProcesses);
        logCollectorForm.ArtifactsToCollectAll.Check();

        foreach (var type in ArtifactsType)
            type.IsChecked.Wait().EqualTo(true);
    }

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(30)]
    [TestCase(60)]
    public void LogsAgeLimit_WorkCorrect(LogsAgeLimi ageLimit)
    {
        // arrange
        GenerateLog(ArtifactsType.RunningProcesses, ageLimit);

        // act
        var logCollectorForm = Application.Run();
        logCollectorForm.LogsAgeLimit.SelectValue(ageLimit);
        logCollectorForm.Collect();

        // assert
        logCollectorForm.LogInfoBlock.Text.Wait().Contains("Finish success");
        CheckLogFileConainsLogs(ArtifactsType.RunningProcesses, LogCollectionMode.Filtered_binary, ageLimit);
    }

    [TestCase("")]
    [TestCase(BasePath + "<>;:|?*]")]
    [TestCase(BasePath + LongFileName)]
    [TestCase(BasePath + IncorrectFileName)]
    [TestCase(IncorrectPath + BaseFileName)]
    [Description("")]
    public void IncorrectLogPath_ErrorView(string path)
    {
        // act
        var logCollectorForm = Application.Run();
        logCollectorForm.LogPathBlock.SetValue(path);
        logCollectorForm.Collect();
        
        // assert
        logCollectorForm.ErrorMessegeWindow.Visible.Wait().EqualTo(true);
        logCollectorForm.LogPathBlock.SetValue(BasePath + BaseFileName);
        logCollectorForm.Collect();
        Wait.For(() => logCollectorForm.LogInfoBlock.Text.Wait().Contains("Finish success"));
    }

    //Filtered_binary
    [TestCase(ArtifactsType.RunningProcesses, LogCollectionMode.Filtered_binary)]
    [TestCase(ArtifactsType.ApplicationEventLog, LogCollectionMode.Filtered_binary)]
    [TestCase(ArtifactsType.SystemEventLog, LogCollectionMode.Filtered_binary)]
    [TestCase(ArtifactsType.SetupAPILogs, LogCollectionMode.Filtered_binary)]
    [TestCase(ArtifactsType.SystemConfiguration, LogCollectionMode.Filtered_binary)]
    [TestCase(ArtifactsType.NetworkConfiguration, LogCollectionMode.Filtered_binary)]
    [TestCase(ArtifactsType.WFT_Filters, LogCollectionMode.Filtered_binary)]
    //Filtered_XML
    [TestCase(ArtifactsType.RunningProcesses, LogCollectionMode.Filtered_XML)]
    [TestCase(ArtifactsType.ApplicationEventLog, LogCollectionMode.Filtered_XML)]
    [TestCase(ArtifactsType.SystemEventLog, LogCollectionMode.Filtered_XML)]
    [TestCase(ArtifactsType.SetupAPILogs, LogCollectionMode.Filtered_XML)]
    [TestCase(ArtifactsType.SystemConfiguration, LogCollectionMode.Filtered_XML)]
    [TestCase(ArtifactsType.NetworkConfiguration, LogCollectionMode.Filtered_XML)]
    [TestCase(ArtifactsType.WFT_Filters, LogCollectionMode.Filtered_XML)]
    //Original_binary_from_disk
    [TestCase(ArtifactsType.RunningProcesses, LogCollectionMode.Original_binary_from_disk)]
    [TestCase(ArtifactsType.ApplicationEventLog, LogCollectionMode.Original_binary_from_disk)]
    [TestCase(ArtifactsType.SystemEventLog, LogCollectionMode.Original_binary_from_disk)]
    [TestCase(ArtifactsType.SetupAPILogs, LogCollectionMode.Original_binary_from_disk)]
    [TestCase(ArtifactsType.SystemConfiguration, LogCollectionMode.Original_binary_from_disk)]
    [TestCase(ArtifactsType.NetworkConfiguration, LogCollectionMode.Original_binary_from_disk)]
    [TestCase(ArtifactsType.WFT_Filters, LogCollectionMode.Original_binary_from_disk)]
    [Description("Collect correct log in correct log mode")]
    public void ChooseLogTypeAndLogMode_Collect_Success(ArtifactsType logType, LogCollectionMode mode)
    {
        // arrange
        GenerateLog(logType, 1);

        // act
        var logCollectorForm = Application.Run();
        logCollectorForm.ArtifactsToCollectAll.Uncheck();
        logCollectorForm.ArtifactsToCollect.Select(logType);
        logCollectorForm.LogCollectionMode.Select(mode);
        logCollectorForm.Collect();

        // assert
        Wait.For(() => logCollectorForm.LogInfoBlock.Text.Wait().Contains("Finish success"));
        CheckLogFileConainsLogs(logType, mode, 1);
    }
}
