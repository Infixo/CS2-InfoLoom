import React from 'react';
import { useDataUpdate } from 'hookui-framework'
import $Panel from './panel'

const AlignedParagraph = ({ left, right }) => {
  const containerStyle = {
    display: 'flex',
    justifyContent: 'space-between',
    textAlign: 'justify',
	marginBottom: '0.25em', // Add some spacing between the <p> tags
  };
  const leftTextStyle = {
    width: '40%',
	marginLeft: '10%', // Start 10% from the left edge
  };
  const rightTextStyle = {
    width: '40%',
	marginRight: '10%', // Start 10% from the right edge
    textAlign: 'right',
  };	
	return (
		<div class="labels_L7Q row_S2v" style={{width: '95%', paddingTop: '1rem', paddingBottom: '1rem', lineHeight: '100%', marginTop: '2rem' }}>
			<div class="row_S2v"           style={{width: '60%', justifyContent: 'center'}}>{left}</div>
			<div class="row_S2v"           style={{width: '40%', justifyContent: 'center'}}>{right}</div>
		</div>
	);
};

// length - maximum length that corresponds with base
// base - represents entire bar
// xpos, ypos - location of the bar
const PopulationBarSVG = ({ react, xpos, ypos, length, base, info}) => {
	const barH = 12;
	const barY = ypos + info.age * barH; // TODO: bar height
	const x_work = length*info.work/base;
	const x_school1 = length*info.school1/base;
	const x_school2 = length*info.school2/base;
	const x_school3 = length*info.school3/base;
	const x_school4 = length*info.school4/base;
	const x_other = length*info.other/base;
	
	return (
	  <>
		<text y={barY+barH-1} x={xpos-90} fill='white' fontSize={barH-1} textAnchor='middle'>{info.age}</text>
		<text y={barY+barH-1} x={xpos-50} fill='white' fontSize={barH-1} textAnchor='middle'>{info.total}</text>
		<rect y={barY} height={barH} fill='#99E2FF' width={x_work}    x={xpos} /> // work, pastel blue
		<rect y={barY} height={barH} fill='#DAFF7F' width={x_school1} x={xpos+x_work} /> // elementary, pale lime
		<rect y={barY} height={barH} fill='#7FFF8E' width={x_school2} x={xpos+x_work+x_school1} /> // high school, mint green
		<rect y={barY} height={barH} fill='#7F92FF' width={x_school3} x={xpos+x_work+x_school1+x_school2} /> // college, light blue
		<rect y={barY} height={barH} fill='#D67FFF' width={x_school4} x={xpos+x_work+x_school1+x_school2+x_school3} /> // university, lavender blue
		<rect y={barY} height={barH} fill='#C09881' width={x_other}   x={xpos+x_work+x_school1+x_school2+x_school3+x_school4} /> // other, light brown
	  </>
	);
};

// length - maximum length that corresponds with base
// base - represents entire bar
// xpos, ypos - location of the bar
const PopulationBar = ({ legend, length, base, info, barH}) => {
	const x_age = legend * 20/100;
	const x_total = legend * 80/100;
	const x_work = length*info.work/base;
	const x_school1 = length*info.school1/base;
	const x_school2 = length*info.school2/base;
	const x_school3 = length*info.school3/base;
	const x_school4 = length*info.school4/base;
	const x_other = length*info.other/base;

	return (
		<div style={{ display: 'flex' }}>
			<div style={{ width: `${x_age}px`,   display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: `${barH-2}px` }}>{info.age}</div>
			<div style={{ width: `${x_total}px`, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: `${barH-2}px` }}>{info.total}</div>
			<div style={{ height: `${barH}px`, width: `${x_work}px`,    backgroundColor: '#C09881' }}></div>{/* light brown */}
			<div style={{ height: `${barH}px`, width: `${x_school1}px`, backgroundColor: '#DAFF7F' }}></div>{/* pale lime   */}
			<div style={{ height: `${barH}px`, width: `${x_school2}px`, backgroundColor: '#7FFF8E' }}></div>{/* mint green  */}
			<div style={{ height: `${barH}px`, width: `${x_school3}px`, backgroundColor: '#51FFE4' }}></div>{/* turquoise   */}
			<div style={{ height: `${barH}px`, width: `${x_school4}px`, backgroundColor: '#2361FF' }}></div>{/* bright blue */}
			<div style={{ height: `${barH}px`, width: `${x_other}px`,   backgroundColor: '#D8D8D8' }}></div>{/* silver gray */}
		</div>
	);
};


const $Demographics = ({react}) => {
	
    // 0 - num citizens in the city 0 = 1+2+3
    // 1 - num locals
    // 2 - num tourists
    // 3 - num commuters
    // 4 - num students (in locals) 4 <= 1
    // 5 - num workers (in locals) 5 <= 1
    // 6 - oldest cim
	const [totals, setTotals] = react.useState([])
	useDataUpdate(react, 'populationInfo.structureTotals', setTotals)

	const [details, setDetails] = react.useState([])
	useDataUpdate(react, 'populationInfo.structureDetails', setDetails)

	const onClose = () => {
		const data = { type: "toggle_visibility", id: 'infoloom.demographics' };
		const event = new CustomEvent('hookui', { detail: data });
		window.dispatchEvent(event);
	};
	
	const panWidth = window.innerWidth * 0.20;
	const panHeight = window.innerHeight * 0.86;
	const barHeight = panHeight * 0.77 / 110; // TODO: 110 is number of bars - should correspond with backend
	const lineSpan = panWidth * 0.9 / 5; // 1000 pop lines span
	
  const gridLines = Array.from({ length: 5 }, (_, index) => (
    <line key={index}
      x1={panWidth*0.1 + lineSpan*(index+1)} y1="0"
      x2={panWidth*0.1 + lineSpan*(index+1)} y2={panHeight * 0.83}
      stroke="white"  strokeWidth="1"
    />
  ));	

	return <$Panel react={react} title="Demographics" onClose={onClose} initialSize={{ width: panWidth, height: panHeight }} initialPosition={{ top: window.innerHeight * 0.009, left: window.innerWidth * 0.053 }}>
	
	  <div style={{ display: 'flex', flexDirection: 'row' }}>
		<div style={{width: '50%'}} >
			<AlignedParagraph left="All Citizens" right={totals[0]} />
			<AlignedParagraph left="- Locals" right={totals[1]} />
			<AlignedParagraph left="- Tourists" right={totals[2]} />
			<AlignedParagraph left="- Commuters" right={totals[3]} />
			<AlignedParagraph left="Moving Away" right={totals[7]} />
		</div>
		<div style={{width: '50%'}} >
			<AlignedParagraph left="Oldest citizen" right={totals[6]} />
			<AlignedParagraph left="Students" right={totals[4]} />
			<AlignedParagraph left="Workers" right={totals[5]} />
			<AlignedParagraph left="Homeless" right="t.b.d." />
			<AlignedParagraph left="Dead" right={totals[8]} />
		</div>
	  </div>
	  <div style={{height: '3rem'}}></div>	  
	  <div style={{display: 'flex', flexDirection: 'row', alignItems: 'center', justifyContent: 'space-around', fontSize: `${barHeight+2}px` }}>
		<div style={{ height: `${barHeight}px`, width: `${barHeight*3}px`, backgroundColor: '#C09881' }}/><div>Work</div>{/* light brown */}
		<div style={{ height: `${barHeight}px`, width: `${barHeight*3}px`, backgroundColor: '#DAFF7F' }}/><div>Elementary</div>{/* pale lime */}
		<div style={{ height: `${barHeight}px`, width: `${barHeight*3}px`, backgroundColor: '#7FFF8E' }}/><div>High school</div>{/* mint green */}
		<div style={{ height: `${barHeight}px`, width: `${barHeight*3}px`, backgroundColor: '#51FFE4' }}/><div>College</div>{/* turquoise  */}
		<div style={{ height: `${barHeight}px`, width: `${barHeight*3}px`, backgroundColor: '#2361FF' }}/><div>University</div>{/* bright blue */}
		<div style={{ height: `${barHeight}px`, width: `${barHeight*3}px`, backgroundColor: '#D8D8D8' }}/><div>Other</div>{/* silver gray */}
	  </div>
	  <div style={{height: '3rem'}}></div>		
	  <svg width={panWidth} height={panHeight*0.85}>
		{gridLines}
		{(() => {
			const bars = [];
			details.forEach( info => {
				bars.push(
					<PopulationBar key={info.age} legend={panWidth*0.1} length={panWidth*0.9} base={5000} info={info} barH={barHeight}/>
				);
			});
			return bars;
		})()}
	  </svg>
	  
		{/* DEBUG
		<div>
		{(() => {
			const paragraphs = [];
			details.forEach( info => {
				paragraphs.push( <p style={{ fontSize: '75%' }} key={info["age"]}> {info["age"]}  {info["total"]}  {info["school1"]}  {info["school2"]}  {info["school3"]}  {info["school4"]}  {info["work"]}  {info["other"]}</p> );
			});
			return paragraphs;
		})()}
		</div>
		*/}
		
	</$Panel>
}

// Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "infoloom.demographics",
    name: "InfoLoom: Demographics",
    icon: "Media/Game/Icons/Population.svg",
    component: $Demographics
})

