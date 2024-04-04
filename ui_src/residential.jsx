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
		width: right2 === undefined ? '30%' : '15%',
		justifyContent: 'center',
	};
	const right1text = `${right1}`;
	const right2text = `${right2}`;
	return (
	<div class="labels_L7Q row_S2v">
		<div class="row_S2v" style={{width: '70%', flexDirection: 'column'}}>
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

const BuildingDemandSection = ({ data }) => {
	const freeL = data[0]-data[3];
	const freeM = data[1]-data[4];
	const freeH = data[2]-data[5];
	const ratio = data[6]/10;
	const ratioString = `$No demand at {ratio}%`;
	const needL = Math.max(1, Math.floor(ratio * data[0] / 100));
	const needM = Math.max(1, Math.floor(ratio * data[1] / 100));
	const needH = Math.max(1, Math.floor(ratio * data[2] / 100));
	const demandL = Math.floor((1 - freeL / needL) * 100);
	const demandM = Math.floor((1 - freeM / needM) * 100);
	const demandH = Math.floor((1 - freeH / needH) * 100);
	return (
	<div style={{boxSizing: 'border-box', border: '1px solid gray'}}>
		<div class="labels_L7Q row_S2v">
			<div class="row_S2v" style={{width: '40%'}}></div>
			<SingleValue value="LOW" />
			<SingleValue value="MEDIUM" />
			<SingleValue value="HIGH" />
		</div>
		<div class="labels_L7Q row_S2v">
			<div class="row_S2v" style={{width: '2%'}}></div>
			<div class="row_S2v" style={{width: '38%'}}>Total properties</div>
			<SingleValue value={data[0]} />
			<SingleValue value={data[1]} />
			<SingleValue value={data[2]} />
		</div>
		<div class="labels_L7Q row_S2v">
			<div class="row_S2v small_ExK" style={{width: '2%'}}></div>
			<div class="row_S2v small_ExK" style={{width: '38%'}}>- Occupied properties</div>
			<SingleValue value={data[3]} small={true} />
			<SingleValue value={data[4]} small={true} />
			<SingleValue value={data[5]} small={true} />
		</div>
		<div class="labels_L7Q row_S2v">
			<div class="row_S2v" style={{width: '2%'}}></div>
			<div class="row_S2v" style={{width: '38%'}}>= Empty properties</div>
			<SingleValue value={freeL} flag={freeL>needL} />
			<SingleValue value={freeM} flag={freeM>needM} />
			<SingleValue value={freeH} flag={freeH>needH} />
		</div>
		<div class="labels_L7Q row_S2v">
			<div class="row_S2v small_ExK" style={{width: '2%'}}></div>
			<div class="row_S2v small_ExK" style={{width: '38%'}}>{"No demand at " + ratio + "%"}</div>
			<SingleValue value={needL} small={true} />
			<SingleValue value={needM} small={true} />
			<SingleValue value={needH} small={true} />
		</div>
		<div class="labels_L7Q row_S2v">
			<div class="row_S2v" style={{width: '2%'}}></div>
			<div class="row_S2v" style={{width: '38%'}}>BUILDING DEMAND</div>
			<SingleValue value={demandL} flag={demandL<0} />
			<SingleValue value={demandM} flag={demandM<0} />
			<SingleValue value={demandH} flag={demandH<0} />
		</div>
	    <div class="space_uKL" style={{height: '3rem'}}></div>
	</div>
	);
};

const $Residential = ({ react }) => {
	
	// residential data
	const [residentialData, setResidentialData] = react.useState([])
	useDataUpdate(react, 'cityInfo.ilResidential', setResidentialData)

	const onClose = () => {
		// HookUI
		//const data = { type: "toggle_visibility", id: 'infoloom.residential' };
		//const event = new CustomEvent('hookui', { detail: data });
		//window.dispatchEvent(event);
		// Gooee
        engine.trigger("infoloom.infoloom.OnToggleVisibleResidential", "Residential");
        engine.trigger("audio.playSound", "close-panel", 1);
	};
	
	const homelessThreshold = Math.round(residentialData[12] * residentialData[13] / 1000);

	return <$Panel react={react} title="Residential Data" onClose={onClose} initialSize={{ width: window.innerWidth * 0.22, height: window.innerHeight * 0.26 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>	
		{residentialData.length === 0 ? (
			<p>Waiting...</p>
		) : (
		<div>
			<BuildingDemandSection data={residentialData} />
			{/* OTHER DATA, two columns */}
			<div style={{display: 'flex'}}>
				<div style={{width: '50%', boxSizing: 'border-box', border: '1px solid gray'}}>
					<div class="space_uKL" style={{height: '3rem'}}></div>
					<RowWithTwoColumns left="STUDY POSITIONS" right={residentialData[14]} />
					<DataDivider />
					<RowWithThreeColumns left="HAPPINESS" leftSmall={`${residentialData[8]} is neutral`} right1={residentialData[7]} flag1={residentialData[7]<residentialData[8]} />
					<DataDivider />
					<RowWithThreeColumns left="UNEMPLOYMENT" leftSmall={`${residentialData[10]/10}% is neutral`} right1={residentialData[9]} flag1={residentialData[9]>residentialData[10]/10} />
					<DataDivider />
					<RowWithThreeColumns left="HOUSEHOLD DEMAND" right1={residentialData[16]} flag1={residentialData[16]<0} />
					<div class="space_uKL" style={{height: '3rem'}}></div>
				</div>
				<div style={{width: '50%', boxSizing: 'border-box', border: '1px solid gray'}}>
					<div class="space_uKL" style={{height: '3rem'}}></div>
					<RowWithTwoColumns left="HOUSEHOLDS" right={residentialData[12]} />
					<DataDivider />
					<RowWithThreeColumns left="HOMELESS" leftSmall={`${homelessThreshold} is neutral`} right1={residentialData[11]} flag1={residentialData[11]>homelessThreshold} />
					<DataDivider />
					<RowWithThreeColumns left="TAX RATE (weighted)" leftSmall="10% is neutral" right1={residentialData[15]/10} flag1={residentialData[15]>100} />
					<DataDivider />
					<RowWithTwoColumns left="STUDENT CHANCE" right={`${residentialData[17]} %`} />
					<div class="space_uKL" style={{height: '3rem'}}></div>
				</div>
			</div>
		</div>
		)}
	</$Panel>
};

export default $Residential

// Registering the panel with HookUI so it shows up in the menu
/*
window._$hookui.registerPanel({
	id: "infoloom.residential",
	name: "InfoLoom: Residential Data",
	icon: "Media/Game/Icons/ZoneResidential.svg",
	component: $Residential
});
*/