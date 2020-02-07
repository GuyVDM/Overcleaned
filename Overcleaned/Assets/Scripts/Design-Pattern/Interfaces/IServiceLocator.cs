using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IServiceLocator
{

    event Action OnAddedService;

    event Action OnRemovedSerice;

}
