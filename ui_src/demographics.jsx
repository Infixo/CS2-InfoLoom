import React, {useState} from 'react';
import { useDataUpdate } from 'hookui-framework'
import $Panel from './panel'
//import { useState, useEffect, useRef } from 'react';

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
    <p style={containerStyle}>
      <span style={leftTextStyle}>{left}</span>
      <span style={rightTextStyle}>{right}</span>
    </p>
  );
};

const HorizontalLine = ({ length }) => {
  const lineStyle = {
    width: `${length}px`,
    borderBottom: '5px solid white', // Adjust the border style as needed
    margin: '1px 0', // Adjust the margin as needed
  };
  return <div style={lineStyle}></div>;
};


const HorizontalLine2 = ({ length, strokeWidth, color }) => {
  return (
    <div>
    <svg height="2" width={length}>
      <line x1="0" y1="1" x2={length} y2="1" stroke={color} strokeWidth={strokeWidth} />
    </svg>
	</div>
  );
};

const CanvasWithSquares = () => {
  const canvasRef = useRef(null);

  react.useEffect(() => {
    const canvas = canvasRef.current;
    const context = canvas.getContext('2d');

    // Draw the first square
    context.fillStyle = 'blue';
    context.fillRect(50, 50, 100, 100);

    // Draw the second square
    context.fillStyle = 'green';
    context.fillRect(200, 50, 100, 100);
  }, []); // The empty dependency array ensures that this effect runs only once after the initial render

  return (
    <canvas
      ref={canvasRef}
      width={400}
      height={200}
      style={{ border: '1px solid black' }}
    ></canvas>
  );
};

// length - maximum length that corresponds with base
// base - represents entire bar
// xpos, ypos - location of the bar
const PopulationBar = ({ react, xpos, ypos, length, base, info}) => {
	const [svgContent, setSvgContent] = react.useState(
    "<text x='100' y='100' fontSize='20' fill='red'>" + info.total + "</text>"
	);
	//console.log(svgContent);

	//const [total, setTotal] = react.useState(info.total);
	
	//if (total !== info.total)
	//{
		//setTotal(info.total);
		//console.log("new total", info.total, total);
	//}
  
	//console.log("bar", info);
	const barH = 12;
	const barY = ypos + info.age * barH; // TODO: bar height
	const x_work = length*info.work/base;
	const x_school1 = length*info.school1/base;
	const x_school2 = length*info.school2/base;
	const x_school3 = length*info.school3/base;
	const x_school4 = length*info.school4/base;
	const x_other = length*info.other/base;
	const test1 = "<text x=0 y="+barY+" fontSize=12 fill='red'>" + info.total + "</text>"
	console.log(test1);
		//<text y={barY+barH-1} x={xpos-90} fill="white" fontSize={barH-1} textAnchor="middle">{info.age}</text>
		//<text y={barY+barH-1} x={xpos-50} fill="white" fontSize={barH-1} textAnchor="middle">{info.total}</text>
		//<rect y={barY} height={barH} fill='#99E2FF' width={x_work}    x={xpos}/> // work
		//<rect y={barY} height={barH} fill='#DAFF7F' width={x_school1} x={xpos+x_work} /> // elementary
		//<rect y={barY} height={barH} fill='#7FFF8E' width={x_school2} x={xpos+x_work+x_school1} /> // high school
		//<rect y={barY} height={barH} fill='#7F92FF' width={x_school3} x={xpos+x_work+x_school1+x_school2} /> // college
		//<rect y={barY} height={barH} fill='#D67FFF' width={x_school4} x={xpos+x_work+x_school1+x_school2+x_school3} /> // university
		//<rect y={barY} height={barH} fill='#C09881' width={x_other} x={xpos+x_work+x_school1+x_school2+x_school3+x_school4} /> // other
	
	
	return (
		<div dangerouslySetInnerHTML={{ __html: test1 }} />	
	);
};


const $Demographics = ({react}) => {
	
    const [oldestCim, setOldestCim] = react.useState(0)
    useDataUpdate(react, 'populationInfo.oldest_citizen', setOldestCim)
	
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


	// TEST
	const numbers = Array.from({ length: 20 }, (_, index) => index + 1)

	const lines = [];
	for (let i = 0; i < numbers.length; i++) {
		lines.push(<HorizontalLine key={i} length={numbers[i]*50} />);
	}
	//paragraphs.push( <HorizontalLine key={i} length={totals[i]/100} /> );

	const onClose = () => {
		const data = { type: "toggle_visibility", id: 'infoloom.demographics' };
		const event = new CustomEvent('hookui', { detail: data });
		window.dispatchEvent(event);
	};

	return <$Panel react={react} title="Demographics" onClose={onClose} initialSize={{ width: window.innerWidth * 0.2, height: window.innerHeight * 0.4 }} initialPosition={{ top: window.innerHeight * 0.05, left: window.innerWidth * 0.005 }}>
	
	
		<div>
			<AlignedParagraph left="All Citizens" right={totals[0]} />
			<AlignedParagraph left="- Locals" right={totals[1]} />
			<AlignedParagraph left="- Tourists" right={totals[2]} />
			<AlignedParagraph left="- Commuters" right={totals[3]} />
			<AlignedParagraph left="Students" right={totals[4]} />
			<AlignedParagraph left="Workers" right={totals[5]} />
			<AlignedParagraph left="Oldest citizen" right={totals[6]} />
		</div>
		
		<svg width='100%' height='50%' >
		{(() => {
			const bars = [];
			details.forEach( info => {
				bars.push(
					//<rect key={info.age} x='10' y={info.age * 10} width={info.total / 20} height='10' fill="yellow" />
					<PopulationBar key={info.age} react={react} xpos={100} ypos={0} length={500} base={7000} info={info} />
				);
			});
			return bars;
		})()}
			
		</svg>
		
		<div>
		{(() => {
			const paragraphs = [];
			details.forEach( info => {
				paragraphs.push( <p style={{ fontSize: '75%' }} key={info["age"]}> {info["age"]}  {info["total"]}  {info["school1"]}  {info["school2"]}  {info["school3"]}  {info["school4"]}  {info["work"]}  {info["other"]}</p> );
			});
			return paragraphs;
		})()}
		</div>
		
	</$Panel>
}

// Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "infoloom.demographics",
    name: "InfoLoom: Demographics",
    icon: "Media/Game/Icons/Population.svg",
    component: $Demographics
})

