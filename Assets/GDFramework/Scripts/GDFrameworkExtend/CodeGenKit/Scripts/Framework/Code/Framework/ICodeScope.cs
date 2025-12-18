/****************************************************************************
 * Copyright (c) 2015 ~ 2022 liangxiegame UNDER MIT LICENSE
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

namespace GDFrameworkExtend.CodeGenKit
{
    using System.Collections.Generic;

    public interface ICodeScope : ICode
    {
        List<ICode> Codes { get; set; }
    }
}