namespace MawMediaMigrate.Move;

interface IMover
{
    IEnumerable<MoveResult> MoveFiles();
}
