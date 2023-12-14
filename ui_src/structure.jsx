import React from 'react'
import {useDataUpdate, $Panel} from 'hookui-framework'

const $Demographics = ({react}) => {
    // This sets up the oldestCim as local state
    const [oldestCim, setOldestCim] = react.useState(0)

    // useDataUpdate binds the result of the GetterValueBinding to oldestCim
    useDataUpdate(react, 'populationInfo.oldest_citizen', setOldestCim)

    return <$Panel react={react} title="Demographics">
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Oldest citizen</div>
            <div>{oldestCim}</div>
        </div>
    </$Panel>
}

// Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "infixo.demographics",
    name: "Demographics",
    icon: "Media/Game/Icons/Population.svg",
    component: $Demographics
})