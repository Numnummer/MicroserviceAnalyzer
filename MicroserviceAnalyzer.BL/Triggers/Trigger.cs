namespace MicroserviceAnalyzer.BL.Triggers;

public abstract class Trigger
{
    protected abstract byte CalculateTriggerPercentage();
    public byte GetTriggerPercentage()
    {
        var triggerPercentage = CalculateTriggerPercentage();
        if (triggerPercentage > 100)
        {
            throw new Exception("Trigger percentage out of range");
        }
        return triggerPercentage;
    }
}