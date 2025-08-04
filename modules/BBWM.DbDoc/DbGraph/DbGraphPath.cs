namespace BBWM.DbDoc.DbGraph;

public interface IDbGraphPath : IEnumerable<DbGraphEdge> { }

public class DbGraphPath : List<DbGraphEdge>, IDbGraphPath { }