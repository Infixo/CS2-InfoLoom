import React from 'react'
import {useDataUpdate} from 'hookui-framework'
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
	const right1text = `${right1}`;
	const right2text = `${right2}`;
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

const ColumnIndustrialData = ({ data }) => {
	return (
	<div style={{width: '70%', boxSizing: 'border-box', border: '1px solid gray'}}>
	
		<div class="labels_L7Q row_S2v">
			<div class="row_S2v" style={{width: '60%'}}></div>
			<SingleValue value="INDUSTRIAL" />
			<SingleValue value="OFFICE" />
		</div>
		
		<RowWithThreeColumns left="EMPTY BUILDINGS" right1={data[0]} right2={data[10]} />
		<RowWithThreeColumns left="PROPERTYLESS COMPANIES" right1={data[1]} right2={data[11]} />
		
		<DataDivider />
		
		<RowWithThreeColumns left="AVERAGE TAX RATE" leftSmall="10% is the neutral rate" right1={data[2]/10} flag1={data[2]>100} right2={data[12]/10} flag2={data[12]>100} />
		
		<DataDivider />
		
		<RowWithThreeColumns left="LOCAL DEMAND" leftSmall="100% when production = demand" right1={data[3]} flag1={data[3]>100} right2={data[13]} flag2={data[13]>100} />
		<RowWithThreeColumns left="INPUT UTILIZATION" leftSmall="110% is the neutral ratio" right1={data[7]} flag1={data[7]>100} />

		<div class="labels_L7Q row_S2v">
			<div class="row_S2v" style={{width: '60%'}} />
			<div class="row_S2v" style={{width: '20%', justifyContent: 'center'}}>Standard</div>
			<div class="row_S2v" style={{width: '20%', justifyContent: 'center'}}>Leisure</div>
		</div>
		
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
			<div style={{width: '60%', height: '2.2em', display: 'flex', alignItems: 'center'}}>
				STORAGE
			</div>
			<div style={{width: '40%'}}>
				<RowWithTwoColumns left="Free buildings" right={data[5]} />
				<RowWithTwoColumns left="Propertyless companies" right={data[6]} />
				<RowWithTwoColumns left="Demanded types" right={data[15]} />
			</div>
		</div>
		
	</div>
	);
};

const ColumnExcludedResources = ({ resources }) => {
	return (
	<div style={{width: '30%', boxSizing: 'border-box', border: '1px solid gray'}}>
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
		const data = { type: "toggle_visibility", id: 'infoloom.industrial' };
		const event = new CustomEvent('hookui', { detail: data });
		window.dispatchEvent(event);
	};

	return <$Panel react={react} title="Industrial and Office Data" onClose={onClose} initialSize={{ width: window.innerWidth * 0.4, height: window.innerHeight * 0.4 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>	
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

// Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
	id: "infoloom.industrial",
	name: "InfoLoom: Industrial and Office Data",
	icon: "Media/Game/Icons/ZoneIndustrial.svg",
	component: $Industrial
});
