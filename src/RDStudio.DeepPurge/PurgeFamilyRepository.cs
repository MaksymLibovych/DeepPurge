using Autodesk.Revit.DB;
using RDStudio.DeepPurge.Repositories;

namespace RDStudio.DeepPurge;

public class PurgeFamilyRepository : IPurgeFamilyRepository
{
    private readonly Document _document;

    public PurgeFamilyRepository(Document document)
    {
        _document = document;
    }
}
