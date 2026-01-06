using EventFlowSharp;
using EventFlowSharp.EVFL;
using EventFlowSharp.ORE;

unsafe {
    byte[] buffer = File.ReadAllBytes(@"D:\bin\.todo\EventFlowSharp\TestData\DmF_SY_SleepHotel.110.bfevfl");
    ref ResEventFlowFile evfl = ref CafeEventFlowFile.GetRes(buffer);
    Console.WriteLine(evfl.Header.GetFileName().ToString());

    evfl.Relocate();

    for (int i = 0; i < evfl.FlowchartCount; i++) {
        ref ResFlowchart flowchart = ref evfl.Flowcharts.GetPtr()[i].Get();
        Console.WriteLine(new StringView(flowchart.Name.Get().Data).ToString());

        ResEntryPoint* entryPoints = flowchart.EntryPoints.GetPtr();

        ref ResDic dic = ref flowchart.EntryPointNames.Get();
        ResDicEntry* entries = dic.GetEntries();
        for (int entryIndex = 1; entryIndex < dic.EntryCount; entryIndex++) {
            ref var entry = ref entries[entryIndex];
            Console.WriteLine(entry.GetKey().ToString());

            ref var entryPoint = ref entryPoints[i];
            Console.WriteLine(entryPoint.SubFlowEventIndicesCount);
            Console.WriteLine(entryPoint.VariableDefsCount);
        }
    }
}