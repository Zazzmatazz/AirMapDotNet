# EntityCollection(*T*).IndexOf Method 
 

Determines the index of a specific item in the <a href="http://msdn2.microsoft.com/en-us/library/5y536ey6" target="_blank">IList(T)</a>.

**Namespace:**&nbsp;<a href="N_AirMapDotNet_Entities">AirMapDotNet.Entities</a><br />**Assembly:**&nbsp;AirMapDotNet (in AirMapDotNet.dll) Version: 1.0.6313.34627 (1.0)

## Syntax

**C#**<br />
``` C#
public int IndexOf(
	T item
)
```

**VB**<br />
``` VB
Public Function IndexOf ( 
	item As T
) As Integer
```

**C++**<br />
``` C++
public:
virtual int IndexOf(
	T item
) sealed
```

**F#**<br />
``` F#
abstract IndexOf : 
        item : 'T -> int 
override IndexOf : 
        item : 'T -> int 
```


#### Parameters
&nbsp;<dl><dt>item</dt><dd>Type: <a href="T_AirMapDotNet_Entities_EntityCollection_1">*T*</a><br />The object to locate in the <a href="http://msdn2.microsoft.com/en-us/library/5y536ey6" target="_blank">IList(T)</a>.</dd></dl>

#### Return Value
Type: <a href="http://msdn2.microsoft.com/en-us/library/td2s409d" target="_blank">Int32</a><br />The index of *item* if found in the list; otherwise, -1.

#### Implements
<a href="http://msdn2.microsoft.com/en-us/library/3w0148af" target="_blank">IList(T).IndexOf(T)</a><br />

## See Also


#### Reference
<a href="T_AirMapDotNet_Entities_EntityCollection_1">EntityCollection(T) Class</a><br /><a href="N_AirMapDotNet_Entities">AirMapDotNet.Entities Namespace</a><br />