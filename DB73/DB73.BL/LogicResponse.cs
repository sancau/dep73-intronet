namespace DB73.BL
{
    // This class describes a generic response from a BL module to the top-level 
    // 2 versions - one returns object as output and another one is generic class
    public class LogicResponse<T>
	    where T : class
    {
        public LogicResponse(bool isDone)
        {
            this.IsDone = isDone;

            this.Message = UIMessageBuilder.GetMessage(isDone.ToString());
            this.Output = new object() as T;
        }

        public LogicResponse(bool isDone, T output)
            : this(isDone)
        {
            this.Output = output;
        }

        public LogicResponse(bool isDone, string caseKey)
            : this(isDone)
        {
            this.Message = UIMessageBuilder.GetMessage(caseKey);
        }

        public LogicResponse(bool isDone, string caseKey, T output)
            : this(isDone, caseKey)
        {
            this.Output = output;
        }

        public bool IsDone { get; private set; }
        public string Message { get; private set; }
        public T Output { get; private set; }
    }

    public class LogicResponse
    {
        public bool IsDone { get; private set; }
        public string Message { get; private set; }
        public object Output { get; private set; }

        public LogicResponse(bool isDone)
        {
            this.IsDone = isDone;

            this.Message = UIMessageBuilder.GetMessage(isDone.ToString());
            this.Output = new object();
        }

        public LogicResponse(bool isDone, object output)
            : this(isDone)
        {
            this.Output = output;
        }

        public LogicResponse(bool isDone, string caseKey)
            : this(isDone)
        {
            this.Message = UIMessageBuilder.GetMessage(caseKey);
        }

        public LogicResponse(bool isDone, string caseKey, object output)
            : this(isDone, caseKey)
        {
            this.Output = output;
        }
    }
}