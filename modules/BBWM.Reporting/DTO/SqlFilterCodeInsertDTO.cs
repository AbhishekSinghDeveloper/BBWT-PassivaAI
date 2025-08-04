using BBWM.Reporting.Enums;

namespace BBWM.Reporting.DTO
{
    public class SqlFilterCodeInsertDTO
    {
        public string VariableName { get; set; }
        public SqlFilterVariableType VariableType { get; set; }
        public int Position { get; set; }
    }
}