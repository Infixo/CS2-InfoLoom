import React from 'react'

const defaultStyle = {
    position: 'absolute',
    width: "300rem",
    height: "600rem",
}

const Resizer = ({ onMouseDown }) => {
    const style = {
        position: 'absolute',
        bottom: 0,
        right: 0,
        width: '20px',
        height: '20px',
        cursor: 'nwse-resize',
        zIndex: 10,
    }
    const triangle = {
        width: 20,
        height: 20,
        opacity: 0.2,
        //background: 'linear-gradient(to right bottom, transparent 0%, transparent 50%, var(--accentColorDark) 50%, var(--accentColorDark) 100%)'
		//background: linear-gradient(45deg, to right, to bottom, transparent 0%, transparent 50%, var(--accentColorDark) 50%, var(--accentColorDark) 100%)
		//background-repeat: no-repeat,
		//background-size: 100% 100%
    }
    return <div style={style} onMouseDown={onMouseDown}>
        {/* <img style={{transform: 'rotate(-15deg)'}} src="Media/Misc/BalloonArrowLeftTop.svg"/> */}
        <div style={triangle}/>
    </div>
};

const $CloseButton = ({onClick}) => {
    return <button className="button_bvQ button_bvQ close-button_wKK" onClick={onClick}>
        <div className="tinted-icon_iKo" style={{maskImage: 'url(Media/Glyphs/Close.svg)', width: 'var(--iconWidth)', height: 'var(--iconHeight)' }}></div>
    </button>
}

const $Panel = ({
    react,
    children,
    title,
    style,
    onClose,
    initialPosition,
    initialSize,
    onPositionChange = () => {},
    onSizeChange = () => {},
}) => {
    // TODO these two should be settable by parent
    const [position, setPosition] = react.useState(initialPosition || { top: 100, left: 10 });
    const [size, setSize] = react.useState(initialSize || { width: 300, height: 600 });

    const initialSizeRef = react.useRef({ width: 0, height: 0 });
    const [dragging, setDragging] = react.useState(false);
    const [resizing, setResizing] = react.useState(false);
    const [rel, setRel] = react.useState({ x: 0, y: 0 }); // Position relative to the cursor

    const onMouseDown = (e) => {
        if (e.button !== 0) return;
        setDragging(true);
        const panelElement = e.target.closest(".panel_YqS");
        const rect = panelElement.getBoundingClientRect();
        setRel({
            x: e.clientX - rect.left,
            y: e.clientY - rect.top,
        });
        e.stopPropagation();
        e.preventDefault();
    }

    const onMouseUp = () => {
        setDragging(false);
        setResizing(false);
        window.removeEventListener('mousemove', onMouseMove);
        window.removeEventListener('mouseup', onMouseUp);
        window.removeEventListener('mousemove', onResizeMouseMove);
    }

    const onMouseMove = (e) => {
        if (!dragging || resizing) return;

        const newTop = e.clientY - rel.y;
        const newLeft = e.clientX - rel.x;

        const newPosition = {
            top: newTop > 0 ? newTop : 0,
            left: newLeft > 0 ? newLeft : 0,
        };

        setPosition(newPosition);
        onPositionChange(newPosition);
        e.stopPropagation();
        e.preventDefault();
    }

    const onResizeMouseDown = (e) => {
        setResizing(true);
        initialSizeRef.current = { width: size.width, height: size.height }; // Store initial size
        setRel({ x: e.clientX, y: e.clientY });
        e.stopPropagation();
        e.preventDefault();
    }

    const onResizeMouseMove = (e) => {
        if (!resizing) return;

        const widthChange = e.clientX - rel.x;
        const heightChange = e.clientY - rel.y;
        const newSize = {
            width: Math.max(initialSizeRef.current.width + widthChange, 100),
            height: Math.max(initialSizeRef.current.height + heightChange, 100)
        };
        setSize(newSize);
        onSizeChange(newSize);
        setRel({ x: e.clientX, y: e.clientY });
        e.stopPropagation();
        e.preventDefault();
    }

    react.useEffect(() => {
        if (dragging || resizing) {
            window.addEventListener('mousemove', dragging ? onMouseMove : onResizeMouseMove);
            window.addEventListener('mouseup', onMouseUp);
        }

        return () => {
            window.removeEventListener('mousemove', dragging ? onMouseMove : onResizeMouseMove);
            window.removeEventListener('mouseup', onMouseUp);
        };
    }, [dragging, resizing]);

    const draggableStyle = {
        ...defaultStyle,
        ...style,
        top: `${position.top}px`,
        left: `${position.left}px`,
        width: `${size.width}px`,
        height: `${size.height}px`
    }

    return (
        <div className="panel_YqS" style={draggableStyle}>
            <div className="header_H_U header_Bpo child-opacity-transition_nkS"
                 onMouseDown={onMouseDown}>
                <div className="title-bar_PF4">
                    <div className="icon-space_h_f"></div>
                    <div className="title_SVH title_zQN">{title}</div>
                    <$CloseButton onClick={onClose}/>
                </div>
            </div>
            <div className="content_XD5 content_AD7 child-opacity-transition_nkS">
                {children}
            </div>
            <Resizer onMouseDown={onResizeMouseDown} />
        </div>
    );
}

export default $Panel