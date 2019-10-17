import React from 'react';
import * as PropTypes from 'prop-types';
import { UIManager, Platform, requireNativeComponent, findNodeHandle } from 'react-native';

// noinspection JSUnusedGlobalSymbols
class SimplePdfView extends React.Component {

    static propTypes = {
        onError: PropTypes.func,
        onEndLoading: PropTypes.func,
        onStartLoading: PropTypes.func,
        backgroundColor: PropTypes.any,
        source: PropTypes.string.isRequired,
        style: PropTypes.any,
    };

    static defaultProps = {
        backgroundColor: 'transparent',
    };

    _nativeRef = null;

    _onChangeNativeRef = (ref) => {
        this._nativeRef = ref;
    };

    onChange = (event) => {
        if (!this.props.onChange) {
            return;
        }
        // noinspection JSUnresolvedVariable
        this.props.onChange((event.nativeEvent || {}).strokes || []);
    };

    onExport = (event) => {
        if (!this.props.onExport) {
            return;
        }
        // noinspection JSUnresolvedVariable
        this.props.onExport((event.nativeEvent || {}).base64 || '');
    };

    runCommand = (name, args = []) => {
        if (this._nativeRef) {
            const handle = findNodeHandle(this._nativeRef);
            if (!handle) {
                throw new Error('Cannot find node handles');
            }
            // noinspection JSUnusedGlobalSymbols
            Platform.select({
                default: () => {
                    // noinspection JSUnresolvedVariable
                    const commandId = UIManager.RNInkCanvas.Commands[name] || 0;
                    if (!commandId) {
                        throw new Error(`Cannot find command ${name} in RNInkCanvas manager!`);
                    }
                    UIManager.dispatchViewManagerCommand(handle, commandId, args);
                },
                ios: () => {
                    // noinspection JSUnresolvedVariable
                    NativeModules.RNInkCanvasManager[name](handle, ...args);
                },
            })();
        } else {
            throw new Error('No ref to RNInkCanvas component, check that component is mounted');
        }
    };

    scrollToPage = (pageIndex = 0, animated = true) => {
        this.runCommand('scrollToPage', {pageIndex, animated});
    };

    render() {
        return (
            <RNSimplePdfView {...(this.props || {})} ref={this._onChangeNativeRef} />
        );
    }
}

const RNSimplePdfView = requireNativeComponent('RNSimplePdfView', SimplePdfView, {
    nativeOnly: {
        onError: true,
        onEndLoading: true,
        onStartLoading: true,
    }
});

export default SimplePdfView;
