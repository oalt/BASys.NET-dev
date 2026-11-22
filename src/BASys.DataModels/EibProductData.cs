namespace BASys.DataModels
{
    /// <summary>
    /// Hardware product of a manufacturer product database.
    /// </summary>
    public class EibProduct
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public override string ToString()
        {
            string result = Name;

            return result;
        }
    }

    /// <summary>
    /// Manufacturer of the product database.
    /// </summary>
    public class EibManufacturer
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Communication object of an application program.
    /// </summary>
    public class EibCommunicationObject
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Function { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Object type (EIS type) as stored in the product database.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        public string ReadEnabled { get; set; } = string.Empty;

        public string WriteEnabled { get; set; } = string.Empty;

        public string CommunicationEnabled { get; set; } = string.Empty;
    }

    /// <summary>
    /// Function of an EIB device (a communication object with an assigned function type).
    /// </summary>
    public class EibDeviceFunction
    {
        /// <summary>
        /// Function types that can be assigned to a communication object.
        /// </summary>
        public static readonly string[] FunctionTypes = { "switching", "relative dimming", "dimming value", "value", "date", "time", "unused" };

        public string Name { get; set; } = string.Empty;

        public string FunctionName { get; set; } = string.Empty;

        public string Type { get; set; } = "unused";

        public int ComObject { get; set; }

        public int EisType { get; set; }
    }
}
