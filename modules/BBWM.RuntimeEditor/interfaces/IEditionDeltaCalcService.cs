using BBWM.Core.Membership.Model;

namespace BBWM.RuntimeEditor.interfaces;

public interface IEditionDeltaCalcService
{
    /// <summary>
    /// Calculate a difference between a new edition and the previous
    /// and outputs an edition update that is sent to GIT.
    /// The dictionary is used to fetch original phrases if the edition update needs to roll back the changes
    /// when user undoes them. 
    /// </summary>
    /// <param name="previousEdition">Previous edition</param>
    /// <param name="newEdition">New edition</param>
    /// <param name="dictionary">Markup phrases dictionary</param>
    /// <param name="submittedBy">User who submitted the edition</param>
    /// <returns></returns>
    RteEditionUpdate GetEditionUpdate(RteEdition previousEdition, RteEdition newEdition, RteDictionary dictionary, User submittedBy);
}
