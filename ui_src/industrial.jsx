import React from 'react'
import useDataUpdate from './use-data-update.js'
import $Panel from './panel'

const RowWithTwoColumns = ({left, right}) => {
	return (
	<div class="labels_L7Q row_S2v">
		<div class="row_S2v" style={{width: '70%'}}>{left}</div>
		<div class="row_S2v" style={{width: '30%', justifyContent: 'center'}}>{right}</div>
	</div>
	);
};

const RowWithThreeColumns = ({left, leftSmall, right1, flag1, right2, flag2}) => {
	const centerStyle = {
		width: right2 === undefined ? '40%' : '20%',
		justifyContent: 'center',
	};
	const right1text = `${right1} %`;
	const right2text = `${right2} %`;
	return (
	<div class="labels_L7Q row_S2v">
		<div class="row_S2v" style={{width: '60%', flexDirection: 'column'}}>
			<p>{left}</p>
			<p style={{fontSize: '80%'}}>{leftSmall}</p>
		</div>
		{flag1 ?
			<div class="row_S2v negative_YWY" style={centerStyle}>{right1text}</div> :
			<div class="row_S2v positive_zrK" style={centerStyle}>{right1text}</div>}
		{right2 && (
		flag2 ?
			<div class="row_S2v negative_YWY" style={centerStyle}>{right2text}</div> :
			<div class="row_S2v positive_zrK" style={centerStyle}>{right2text}</div>)}
	</div>
	);
};

// simple horizontal line
const DataDivider = () => {
	return (
	<div style={{display: 'flex', height: '4rem', flexDirection: 'column', justifyContent: 'center'}}>
		<div style={{borderBottom: '1px solid gray'}}></div>
	</div>
	);
};

// centered value, if flag exists then uses colors for negative/positive
// width is 20% by default
const SingleValue = ({ value, flag, width, small }) => {
	const rowClass = ( small ? "row_S2v small_ExK" : "row_S2v");
	const centerStyle = {
		width: width === undefined ? '20%' : width,
		justifyContent: 'center',
	};
	return (
		flag === undefined ? (
			<div class={rowClass}              style={centerStyle}>{value}</div>
		) : (
		flag ?
			<div class={rowClass + " negative_YWY"} style={centerStyle}>{value}</div> :
			<div class={rowClass + " positive_zrK"} style={centerStyle}>{value}</div>)
	);
};

const ColumnIndustrialData = ({ data }) => {
	return (
	<div style={{width: '75%', boxSizing: 'border-box', border: '1px solid gray'}}>
	
		<div class="labels_L7Q row_S2v">
			<div class="row_S2v" style={{width: '60%'}}></div>
			<SingleValue value="INDUSTRIAL" />
			<SingleValue value="OFFICE" />
		</div>
		
		<div class="labels_L7Q row_S2v">
			<div class="row_S2v" style={{width: '60%'}}>EMPTY BUILDINGS</div>
			<SingleValue value={data[0]} />
			<SingleValue value={data[10]} />
		</div>
		<div class="labels_L7Q row_S2v">
			<div class="row_S2v" style={{width: '60%'}}>PROPERTYLESS COMPANIES</div>
			<div class="row_S2v" style={{width: '20%', justifyContent: 'center'}}>{data[1]}</div>
			<div class="row_S2v" style={{width: '20%', justifyContent: 'center'}}>{data[11]}</div>
		</div>
		
		<DataDivider />
		
		<RowWithThreeColumns left="AVERAGE TAX RATE" leftSmall="10% is the neutral rate" right1={data[2]/10} flag1={data[2]>100} right2={data[12]/10} flag2={data[12]>100} />
		
		<DataDivider />
		
		<RowWithThreeColumns left="LOCAL DEMAND (ind)" leftSmall="100% when production = demand" right1={data[3]} flag1={data[3]>100} />
		<RowWithThreeColumns left="INPUT UTILIZATION (ind)" leftSmall="110% is the neutral ratio, capped at 400%" right1={data[7]} flag1={data[7]>100} />
		
		<DataDivider />
		
		<RowWithThreeColumns left="EMPLOYEE CAPACITY RATIO" leftSmall="72% is the neutral ratio" right1={data[4]/10} flag1={data[4]<720} right2={data[14]/10} flag2={data[14]<750} />
		
		<DataDivider />
		
		<div style={{display: 'flex'}}>
			<div style={{width: '60%', height: '2.2em', display: 'flex', alignItems: 'center'}}>
				AVAILABLE WORKFORCE
			</div>
			<div style={{width: '40%'}}>
				<RowWithTwoColumns left="Educated" right={data[8]} />
				<RowWithTwoColumns left="Uneducated" right={data[9]} />
			</div>
		</div>
		
		<DataDivider />
		
		<div style={{display: 'flex'}}>
			<div style={{width: '50%', height: '2.2em', display: 'flex', flexDirection: 'column'}}>
				<p>STORAGE</p>
				<p style={{fontSize: '80%'}}>The game will spawn warehouses when DEMANDED TYPES exist.</p>
			</div>
			<div style={{width: '50%'}}>
				<RowWithTwoColumns left="Empty buildings" right={data[5]} />
				<RowWithTwoColumns left="Propertyless companies" right={data[6]} />
				<RowWithTwoColumns left="DEMANDED TYPES" right={data[15]} />
			</div>
		</div>
		
	</div>
	);
};

const ColumnExcludedResources = ({ resources }) => {
	return (
	<div style={{width: '25%', boxSizing: 'border-box', border: '1px solid gray'}}>
		<div class="row_S2v">No demand for:</div>
        <ul>
          {resources.map((item, index) => (
            <li key={index}>
				<div class="row_S2v small_ExK">{item}</div>
			</li>
          ))}
        </ul>
	</div>
	);
};

const $Industrial = ({ react }) => {

	// commercial data
	const [industrialData, setIndustrialData] = react.useState([])
	useDataUpdate(react, 'cityInfo.ilIndustrial', setIndustrialData)
	
	// excluded resources
	const [excludedResources, setExcludedResources] = react.useState([])
	useDataUpdate(react, 'cityInfo.ilIndustrialExRes', setExcludedResources)

	const onClose = () => {
		// HookUI
		//const data = { type: "toggle_visibility", id: 'infoloom.industrial' };
		//const event = new CustomEvent('hookui', { detail: data });
		//window.dispatchEvent(event);
		// Gooee
        engine.trigger("infoloom.infoloom.OnToggleVisibleIndustrial");
        engine.trigger("audio.playSound", "close-panel", 1);
	};

	return <$Panel react={react} title="Industrial and Office Data" onClose={onClose} initialSize={{ width: window.innerWidth * 0.30, height: window.innerHeight * 0.32 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>	
		{industrialData.length === 0 ? (
			<p>Waiting...</p>
		) : (
		<div style={{display: 'flex'}}>
			<ColumnIndustrialData data={industrialData} />
			<ColumnExcludedResources resources={excludedResources} />
		</div>
		)}
	</$Panel>
};

export default $Industrial

// Registering the panel with HookUI so it shows up in the menu
/*
window._$hookui.registerPanel({
	id: "infoloom.industrial",
	name: "InfoLoom: Industrial and Office Data",
	icon: "Media/Game/Icons/ZoneIndustrial.svg",
	component: $Industrial
});
*/