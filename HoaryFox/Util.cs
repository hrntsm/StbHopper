using System.Collections.Generic;
using STBDotNet.Elements;
using STBDotNet.Elements.StbModel.StbMember;

namespace HoaryFox
{
    public static class Util
    {
        public static IEnumerable<IFrame> GetFrames(StbElements elements, MemberBase member)
        {
            if (member.GetType() == typeof(List<Column>))
            {
                return elements.Model.Members.Columns;
            }

            if (member.GetType() == typeof(List<Post>))
            {
                return elements.Model.Members.Posts;
            }

            if (member.GetType() == typeof(List<Girder>))
            {
                return elements.Model.Members.Girders;
            }

            if (member.GetType() == typeof(List<Beam>))
            {
                return elements.Model.Members.Beams;
            }

            if (member.GetType() == typeof(List<Beam>))
            {
                return elements.Model.Members.Braces;
            }

            return new List<IFrame>();
        }
    }
}