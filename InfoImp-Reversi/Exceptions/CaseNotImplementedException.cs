namespace InfoImp_Reversi.Exceptions; 

public class CaseNotImplementedException : NotImplementedException {

    public CaseNotImplementedException() : base("Case not implemented") {}
}