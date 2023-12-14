import React from 'react'
import {useDataUpdate, $Panel} from 'hookui-framework'

const $Demographics = ({react}) => {
    // This sets up the oldestCim as local state
    const [oldestCim, setCurrentCount] = react.useState(0)

    // useDataUpdate binds the result of the GetterValueBinding to oldestCim
    useDataUpdate(react, 'populationInfo.oldest_citizen', setCurrentCount)

    return <$Panel react={react} title="Demographics">
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Active vehicles</div>
            <div>{currentCount}</div>
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